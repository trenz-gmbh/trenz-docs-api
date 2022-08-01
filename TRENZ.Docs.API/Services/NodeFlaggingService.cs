using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models;
using TRENZ.Docs.API.Models.Index;

namespace TRENZ.Docs.API.Services;

public class NodeFlaggingService : INodeFlaggingService
{
    /// <inheritdoc />
    public async Task UpdateHasContentFlag(Dictionary<string, NavNode> tree, List<IndexFile> indexFiles)
    {
        foreach (var node in tree.Values)
        {
            if (node.Children == null)
            {
                node.HasContent = true; // nodes without children are always content, otherwise they shouldn't appear in the tree

                continue;
            }

            await UpdateHasContentFlag(node.Children!, indexFiles);

            node.HasContent = indexFiles.Any(f => f.location == node.Location);
        }
    }
}