using Meilidown.Models.Sources;

namespace Meilidown.Interfaces;

public interface ISourcesProvider
{
    IEnumerable<ISource> GetSources();
}