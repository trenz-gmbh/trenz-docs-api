using TRENZ.Docs.API.Models;
using TRENZ.Docs.API.Models.Index;

namespace TRENZ.Docs.API.Interfaces;

public interface INavNodeFlaggingService
{
    Task UpdateHasContentFlagAsync(Dictionary<string, NavNode> tree, List<IndexFile> indexFiles, CancellationToken cancellationToken = default);
}
