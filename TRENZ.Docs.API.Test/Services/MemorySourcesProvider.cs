using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models.Sources;

namespace TRENZ.Docs.API.Test.Services;

public class MemorySourcesProvider : ISourcesProvider
{
    private readonly IEnumerable<ISource> _sources;

    public MemorySourcesProvider(IEnumerable<ISource> sources)
    {
        _sources = sources;
    }

    /// <inheritdoc />
    public IEnumerable<ISource> GetSources()
    {
        return _sources;
    }
}
