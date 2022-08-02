using TRENZ.Docs.API.Models;
using TRENZ.Docs.API.Models.Index;

namespace TRENZ.Docs.API.Interfaces;

public interface INavTreeProvider
{
    Task<NavTree> RebuildAsync(List<IndexFile> indexFiles, CancellationToken cancellationToken = default);

    NavTree Tree { get; }
}