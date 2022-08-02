using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models;
using TRENZ.Docs.API.Models.Index;

namespace TRENZ.Docs.API.Services;

public class NavNodeFlaggingService : INavNodeFlaggingService
{
    /// <inheritdoc />
    public async Task UpdateHasContentFlagAsync(Dictionary<string, NavNode> tree, List<IndexFile> indexFiles, CancellationToken cancellationToken = default)
    {
        foreach (var node in tree.Values)
        {
            if (node.Children == null)
            {
                node.HasContent = true; // nodes without children are always content, otherwise they shouldn't appear in the tree

                continue;
            }

            await UpdateHasContentFlagAsync(node.Children!, indexFiles, cancellationToken);

            node.HasContent = indexFiles.Any(f => f.location == node.Location);
        }
    }
}