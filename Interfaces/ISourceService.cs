using Meilidown.Models.Sources;

namespace Meilidown.Interfaces;

public interface ISourceService<TSource> where TSource : ISource
{
    IEnumerable<TSource> GetSources();

    Task UpdateAsync(TSource source, CancellationToken cancellationToken = default);
}