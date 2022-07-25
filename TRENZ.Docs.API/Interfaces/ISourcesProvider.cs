using TRENZ.Docs.API.Models.Sources;

namespace TRENZ.Docs.API.Interfaces;

public interface ISourcesProvider
{
    IEnumerable<ISource> GetSources();
}