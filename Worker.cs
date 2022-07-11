using System.Text;
using Markdig;
using Markdig.Renderers.Normalize;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Meilidown.Models;
using Meilisearch;

namespace Meilidown
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private readonly MeilisearchClient _client;

        public Worker(ILogger<Worker> logger, IConfiguration configuration, MeilisearchClient client)
        {
            _logger = logger;
            _configuration = configuration;
            _client = client;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Running at {Time}", DateTimeOffset.Now);

                var files = GatherFiles();
                var indexedFiles = ProcessFiles(files);
                await UpdateIndex(indexedFiles, stoppingToken);

#if DEBUG
                Console.ReadKey();
#else
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
#endif
            }
        }

        private IEnumerable<RepositoryFile> GatherFiles()
        {
            _logger.LogInformation("Gathering files...");
            
            foreach (var repository in RepositoryRepository.GetRepositories(_configuration))
            {
                _logger.LogInformation("Updating {Repository}", repository);

                repository.Update();

                _logger.LogInformation("Gathering files from {Repository}", repository);

                foreach (var repositoryFile in repository.FindFiles(new(".*\\.md$")))
                {
                    yield return repositoryFile;
                }
            }
        }

        private IEnumerable<IndexedFile> ProcessFiles(IEnumerable<RepositoryFile> files)
        {
            var markdownPipeline = new MarkdownPipelineBuilder()
                .UseAbbreviations()
                // .UseAutoIdentifiers() // causes random "[]:" at the end
                .UseCitations()
                .UseCustomContainers()
                .UseDefinitionLists()
                .UseEmphasisExtras()
                .UseFigures()
                .UseFooters()
                .UseFootnotes()
                .UseGridTables()
                .UseMathematics()
                .UseMediaLinks()
                .UsePipeTables()
                .UseListExtras()
                .UseTaskLists()
                .UseDiagrams()
                .UseAutoLinks()
                .UseEmojiAndSmiley()
                .UseYamlFrontMatter()
                .UseDiagrams()
                .UseGenericAttributes()
                .Build();

            foreach (var f in files)
            {
                _logger.LogInformation("Processing {File}", f.RelativePath);

                var content = File.ReadAllText(f.AbsolutePath);
                var document = Markdown.Parse(content, markdownPipeline);

                UpdateImageLinks(f, document);
                
                var builder = new StringBuilder();
                var writer = new StringWriter(builder);
                var renderer = new NormalizeRenderer(writer);
                renderer.Render(document);

                yield return new(
                    f.Uid,
                    f.Name,
                    builder.ToString(),
                    0,
                    f.Location
                );
            }
        }

        private static void UpdateImageLinks(RepositoryFile file, MarkdownObject markdownObject)
        {
            foreach (var child in markdownObject.Descendants())
            {
                if (child is not LinkInline { IsImage: true } link) continue;

                var location = string.Join('/', file.RelativePath.Split(Path.DirectorySeparatorChar).SkipLast(1)) + '/' + link.Url;

                link.Url = $"%API_HOST%File/{location}";
                if (link.Reference != null) link.Reference.Url = null;
            }
        }

        private async Task UpdateIndex(IEnumerable<IndexedFile> indexedFiles, CancellationToken cancellationToken)
        {
            var health = await _client.HealthAsync(cancellationToken);
            _logger.LogInformation("Meilisearch is {Status}", health.Status);

            var settings = new Settings
            {
                FilterableAttributes = new[] { "uid", "name", "location", "content" },
                SortableAttributes = new[] { "name", "order", "location" },
                SearchableAttributes = new[] { "name", "location", "content" },
            };

            var filesIndex = _client.Index("files");
            var tasks = new Dictionary<string, Task<TaskInfo>>
            {
                { "Delete previous index", filesIndex.DeleteAllDocumentsAsync(cancellationToken) },
                { "Add new index", filesIndex.AddDocumentsAsync(indexedFiles, cancellationToken: cancellationToken) },
                { "Update index settings", filesIndex.UpdateSettingsAsync(settings, cancellationToken) },
            };

            foreach (var task in tasks)
            {
                var info = await task.Value;
                var result = await _client.WaitForTaskAsync(info.Uid, cancellationToken: cancellationToken);
                _logger.LogInformation("Task '{Name}': {Status}", task.Key, result.Status);

                if (result.Error == null) continue;

                _logger.LogError("{@Error}", string.Join(Environment.NewLine, result.Error.Values));
            }
        }
    }
}