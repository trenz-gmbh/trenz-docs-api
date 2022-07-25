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

        public Worker(ILogger<Worker> logger, IConfiguration configuration, IHostApplicationLifetime lifetime, IIndexingService indexingService, ISourcesProvider sourcesProvider, IFileProcessingService fileProcessingService)
        {
            _logger = logger;
            _configuration = configuration;
            _lifetime = lifetime;
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