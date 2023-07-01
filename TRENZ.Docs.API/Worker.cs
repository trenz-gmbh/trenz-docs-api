using System.Runtime.CompilerServices;
using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models.Sources;

namespace TRENZ.Docs.API
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly IIndexingService _indexingService;
        private readonly ISourcesProvider _sourcesProvider;
        private readonly IFileProcessingService _fileProcessingService;
        private readonly INavTreeProvider _navTreeProvider;

        public Worker(ILogger<Worker> logger, IConfiguration configuration, IHostApplicationLifetime lifetime, IIndexingService indexingService, ISourcesProvider sourcesProvider, IFileProcessingService fileProcessingService, INavTreeProvider navTreeProvider)
        {
            _logger = logger;
            _configuration = configuration;
            _lifetime = lifetime;
            _indexingService = indexingService;
            _sourcesProvider = sourcesProvider;
            _fileProcessingService = fileProcessingService;
            _navTreeProvider = navTreeProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at {Time}", DateTimeOffset.Now);

                await DoReindex(stoppingToken);

                if (_configuration["TrenzDocsApi:OneShot"] == "true")
                {
                    _logger.LogWarning("OneShot mode enabled, stopping application");

                    break;
                }

#if DEBUG
                Console.ReadKey();
#else
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
#endif
            }

            _lifetime.StopApplication();
        }

        public async Task DoReindex(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Reindexing...");

            var sourceFiles = await GatherFiles(cancellationToken).ToListAsync(cancellationToken);
            var tree = await _navTreeProvider.RebuildAsync(sourceFiles, cancellationToken);

            var indexTree = tree.WithoutHiddenNodes().WithoutChildlessContentlessNodes();
            var markdownFiles = sourceFiles
                .Where(file => file.RelativePath.EndsWith(".md"))
                .Select(file => (file, node: indexTree.FindNodeByLocation(file.Location)))
                .Where(tup => tup.node is not null)
                .Select(tup => tup.file);

            var indexFiles = await _fileProcessingService
                .ProcessAsync(markdownFiles, cancellationToken)
                .ToListAsync(cancellationToken);

            await _indexingService.IndexAsync(indexFiles, cancellationToken);

            _logger.LogInformation("Reindexing done");
        }

        private async IAsyncEnumerable<ISourceFile> GatherFiles([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            _logger.LogInformation("Gathering files...");
            
            foreach (var source in _sourcesProvider.GetSources())
            {
                _logger.LogInformation("Updating {Source}", source);

                await source.UpdateAsync(cancellationToken);

                _logger.LogInformation("Gathering files from {Source}", source);

                foreach (var sourceFile in source.FindFiles(new(".+")))
                {
                    yield return sourceFile;
                }
            }

            _logger.LogInformation("File gathering done");
        }
    }
}