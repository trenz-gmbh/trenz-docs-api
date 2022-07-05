using GlobExpressions;
using LibGit2Sharp;
using Markdig;
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
                var files = GatherFiles(stoppingToken);
                await IndexFiles(files, stoppingToken);

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        private static string GetRepositoryPath(IConfiguration config)
        {
            return Path.Combine(Path.GetTempPath(), "Meilidown", config["Path"]);
        }

        private static void UpdateRepository(IConfiguration config, CancellationToken cancellationToken)
        {
            var temp = GetRepositoryPath(config);
            if (!Directory.Exists(temp))
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                Repository.Clone(config["Url"], temp, new()
                    {
                        BranchName = config["Branch"],
                    }
                );
            }

            using var repo = new Repository(temp);
            foreach (var remote in repo.Network.Remotes)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                Commands.Fetch(repo, remote.Name, refSpecs, new(), $"Fetching remote {remote.Name}");
            }

            if (repo.Head.FriendlyName != config["Branch"])
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                Commands.Checkout(repo, config["Branch"]);
            }

            if (cancellationToken.IsCancellationRequested)
                return;

            Commands.Pull(
                repo,
                new("Meilidown", "meilidown@example.com", DateTimeOffset.Now),
                new()
                {
                    MergeOptions = new()
                    {
                        MergeFileFavor = MergeFileFavor.Theirs,
                        FileConflictStrategy = CheckoutFileConflictStrategy.Theirs,
                    },
                }
            );
        }

        private IEnumerable<string> GatherFiles(CancellationToken cancellationToken)
        {
            foreach (var source in _configuration.GetRequiredSection("Sources").GetChildren())
            {
                if (source == null)
                    continue;

                var repositoryConfig = source.GetRequiredSection("Repository");
                UpdateRepository(repositoryConfig, cancellationToken);

                var path = GetRepositoryPath(repositoryConfig);
                foreach (var pattern in source.GetRequiredSection("Paths").GetChildren().Select(s => s.Value))
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    foreach (var file in Glob.Files(path, pattern))
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        yield return Path.Combine(path, file);
                    }
                }
            }
        }

        private async Task IndexFiles(IEnumerable<string> paths, CancellationToken cancellationToken)
        {
            var client = new MeilisearchClient(_configuration["Meilisearch:Url"], _configuration["Meilisearch:ApiKey"]);
            var health = await client.HealthAsync(cancellationToken);
            _logger.LogInformation("Meilisearch is {Status}", health.Status);

            var markdownPipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseEmojiAndSmiley()
                .UseYamlFrontMatter()
                .UseDiagrams()
                .Build();
            foreach (var path in paths)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var content = await File.ReadAllTextAsync(path, cancellationToken);
                var document = Markdown.Parse(content, markdownPipeline);

                _logger.LogInformation("File {Path} has {Lines} lines", path, document.LineCount);
            }
        }
    }
}