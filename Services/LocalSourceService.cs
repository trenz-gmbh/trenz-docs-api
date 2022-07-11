using Meilidown.Interfaces;
using Meilidown.Models.Sources;

namespace Meilidown.Services;

public class LocalSourceService : ISourceService<LocalSource>
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<LocalSourceService> _logger;

    public LocalSourceService(IConfiguration configuration, ILogger<LocalSourceService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc />
    public IEnumerable<LocalSource> GetSources()
    {
        return _configuration
            .GetSection("Sources")
            .GetChildren()
            .Where(s => s["Type"] != null)
            .Where(s => string.Equals(s["Type"], SourceType.Local.GetValue(), StringComparison.InvariantCultureIgnoreCase))
            .Select(s => new LocalSource(s));
    }

    /// <inheritdoc />
    public async Task UpdateAsync(LocalSource source, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Nothing to do.");
    }
}