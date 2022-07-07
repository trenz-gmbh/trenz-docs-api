using Meilidown.Common;
using Meilisearch;

namespace Meilidown.Indexer
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

        private IEnumerable<RepositoryFile> GatherFiles()
        {
            foreach (var repository in RepositoryRepository.GetRepositories(_configuration))
            {
                repository.Update();

                foreach (var repositoryFile in repository.FindFiles("**.md"))
                {
                    yield return repositoryFile;
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

            var indexedFiles = files.Select(f =>
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
                { "Add new index", filesIndex.AddDocumentsAsync(indexedFiles, cancellationToken: cancellationToken) },
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