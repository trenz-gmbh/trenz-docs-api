using TRENZ.Docs.API.Models;
using TRENZ.Docs.API.Models.Sources;

namespace TRENZ.Docs.API.Interfaces;

public interface INavTreeProvider
{
    Task<NavTree> RebuildAsync(List<ISourceFile> files, CancellationToken cancellationToken = default);

    NavTree Tree { get; }
}