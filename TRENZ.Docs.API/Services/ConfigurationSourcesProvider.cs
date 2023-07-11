using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models.Sources;

namespace TRENZ.Docs.API.Services;

public class ConfigurationSourcesProvider : ISourcesProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ISafeFileSystemPathTraversalService _pathTraversalService;

    public ConfigurationSourcesProvider(IConfiguration configuration, ILoggerFactory loggerFactory,
                                        ISafeFileSystemPathTraversalService pathTraversalService)
    {
        _configuration = configuration;
        _loggerFactory = loggerFactory;
        _pathTraversalService = pathTraversalService;
    }

    /// <inheritdoc />
    public IEnumerable<ISource> GetSources()
    {
        return _configuration
            .GetSection("Sources")
            .GetChildren()
            .Where(s => s["Type"] != null)
            .Select<IConfigurationSection, ISource>(s => s["Type"] switch
            {
                "git" => new GitSource(s, _loggerFactory.CreateLogger<GitSource>(),
                                       _pathTraversalService),
                "local" => LocalSource.FromConfiguration(s, _pathTraversalService),
                _ => throw new NotImplementedException($"The source type '{s["Type"]}' is not implemented. Must be one of: 'git', 'local'"),
            });
    }
}