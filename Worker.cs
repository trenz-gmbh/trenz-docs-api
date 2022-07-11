using System.Runtime.CompilerServices;
using Meilidown.Interfaces;
using Meilidown.Models.Sources;

namespace Meilidown
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IIndexingService _indexingService;
        private readonly ISourcesProvider _sourcesProvider;
        private readonly IFileProcessingService _fileProcessingService;

        public Worker(ILogger<Worker> logger, IIndexingService indexingService, ISourcesProvider sourcesProvider, IFileProcessingService fileProcessingService)
        {
            _logger = logger;
            _indexingService = indexingService;
            _sourcesProvider = sourcesProvider;
            _fileProcessingService = fileProcessingService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Running at {Time}", DateTimeOffset.Now);

                var files = GatherFiles(stoppingToken);
                var indexFiles = await _fileProcessingService.ProcessAsync(files, stoppingToken).ToListAsync(cancellationToken: stoppingToken);
                await _indexingService.IndexAsync(indexFiles, stoppingToken);

#if DEBUG
                Console.ReadKey();
#else
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
#endif
            }
        }

        private async IAsyncEnumerable<SourceFile> GatherFiles([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            _logger.LogInformation("Gathering files...");
            
            foreach (var source in _sourcesProvider.GetSources())
            {
                _logger.LogInformation("Updating {Source}", source);

                await source.UpdateAsync(cancellationToken);

                _logger.LogInformation("Gathering files from {Source}", source);

                foreach (var repositoryFile in source.FindFiles(new(".*\\.md$")))
                {
                    yield return repositoryFile;
                }
            }

            _logger.LogInformation("File gathering done");
        }
    }
}