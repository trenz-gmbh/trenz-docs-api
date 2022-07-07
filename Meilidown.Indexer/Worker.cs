using LibGit2Sharp;
using Meilisearch;

namespace Meilidown
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var files = GatherFiles();
                await IndexFiles(files, stoppingToken);

                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }

        private static void UpdateRepository(RepositoryConfiguration config)
        {
            Credentials CredentialsHelper(string path, string username, SupportedCredentialTypes supportedCredentialTypes) =>
                new UsernamePasswordCredentials { Username = config.Username, Password = config.Password };

            var temp = config.Root;
            if (!Directory.Exists(temp))
            {
                Repository.Clone(config.Url, temp, new()
                    {
                        BranchName = config.Branch,
                        CredentialsProvider = CredentialsHelper,
                    }
                );
            }

            using var repo = new Repository(temp);
            foreach (var remote in repo.Network.Remotes)
            {
                var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                Commands.Fetch(repo, remote.Name, refSpecs, new()
                {
                    CredentialsProvider = CredentialsHelper,
                }, $"Fetching remote {remote.Name}");
            }

            if (repo.Head.FriendlyName != config.Branch)
            {
                Commands.Checkout(repo, config.Branch);
            }

            Commands.Pull(
                repo,
                new("Meilidown.Indexer", "meilidown@example.com", DateTimeOffset.Now),
                new()
                {
                    MergeOptions = new()
                    {
                        MergeFileFavor = MergeFileFavor.Theirs,
                        FileConflictStrategy = CheckoutFileConflictStrategy.Theirs,
                    },
                    FetchOptions = new()
                    {
                        CredentialsProvider = CredentialsHelper,
                    },
                }
            );
        }

        private IEnumerable<RepositoryFile> GatherFiles()
        {
            foreach (var source in _configuration.GetRequiredSection("Sources").GetChildren())
            {
                var repositoryConfig = new RepositoryConfiguration(source);
                UpdateRepository(repositoryConfig);

                foreach (var repositoryFile in GatherRepositoryFiles(repositoryConfig))
                {
                    yield return repositoryFile;
                }
            }
        }

        private static IEnumerable<RepositoryFile> GatherRepositoryFiles(RepositoryConfiguration config)
        {
            var root = Path.Combine(config.Root, config.Path);
            return IterateDirectory(config, root, root);
        }

        private static IEnumerable<RepositoryFile> IterateDirectory(RepositoryConfiguration config, string path, string root)
        {
            foreach (var file in Directory.EnumerateFileSystemEntries(path, "**.md", SearchOption.AllDirectories))
            {
                if (File.Exists(file))
                {
                    yield return new(config, Path.GetRelativePath(root, file));

                    continue;
                }

                if (!Directory.Exists(file))
                    continue;

                foreach (var f in IterateDirectory(config, file, root))
                {
                    yield return f;
                }
            }
        }

        private async Task IndexFiles(IEnumerable<RepositoryFile> files, CancellationToken cancellationToken)
        {
            var client = new MeilisearchClient(_configuration["Meilisearch:Url"], _configuration["Meilisearch:ApiKey"]);
            var health = await client.HealthAsync(cancellationToken);
            _logger.LogInformation("Meilisearch is {Status}", health.Status);

            // var markdownPipeline = new MarkdownPipelineBuilder()
            //     .UseAdvancedExtensions()
            //     .UseEmojiAndSmiley()
            //     .UseYamlFrontMatter()
            //     .UseDiagrams()
            //     .Build();

            var fils = files.Select(f =>
            {
                _logger.LogInformation("Processing {File}", f.Location);

                var content = File.ReadAllText(f.AbsolutePath);
                // var document = Markdown.Parse(content, markdownPipeline);
                return new IndexedFile(
                    f.Uid,
                    f.Name,
                    content,
                    0,
                    f.Location
                );
            });

            var settings = new Settings
            {
                FilterableAttributes = new[] { "uid", "name", "location", "content" },
                SortableAttributes = new[] { "name", "order", "location" },
                SearchableAttributes = new[] { "name", "location", "content" },
            };

            var filesIndex = client.Index("files");
            var tasks = new Dictionary<string, Task<TaskInfo>>
            {
                { "Delete previous index", filesIndex.DeleteAllDocumentsAsync(cancellationToken) },
                { "Add new index", filesIndex.AddDocumentsAsync(fils, cancellationToken: cancellationToken) },
                { "Update index settings", filesIndex.UpdateSettingsAsync(settings, cancellationToken) },
            };

            foreach (var task in tasks)
            {
                var info = await task.Value;
                var result = await client.WaitForTaskAsync(info.Uid, cancellationToken: cancellationToken);
                _logger.LogInformation("Task '{Name}': {Status}", task.Key, result.Status);

                if (result.Error == null) continue;

                foreach (var e in result.Error.Values)
                {
                    _logger.LogError("{@Error}", e);
                }
            }
        }
    }
}