using TRENZ.Docs.API.Models;
using TRENZ.Docs.API.Models.Sources;

namespace TRENZ.Docs.API.Interfaces;

public interface INavNodeFlaggingService
{
    Task UpdateHasContentFlagAsync(Dictionary<string, NavNode> tree, List<ISourceFile> files, CancellationToken cancellationToken = default);
}
