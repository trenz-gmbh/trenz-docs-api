using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models.Sources;

namespace TRENZ.Docs.API.Services;

public class ConfigurationSourcesProvider : ISourcesProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILoggerFactory _loggerFactory;
    
    public ConfigurationSourcesProvider(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        _configuration = configuration;
        _loggerFactory = loggerFactory;
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
                "git" => new GitSource(s, _loggerFactory),
                "local" => LocalSource.FromConfiguration(s),
                _ => throw new NotImplementedException($"The source type '{s["Type"]}' is not implemented. Must be one of: 'git', 'local'"),
            });
    }
}