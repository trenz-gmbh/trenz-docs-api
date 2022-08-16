using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models;
using TRENZ.Docs.API.Models.Sources;

namespace TRENZ.Docs.API.Services;

public class NavNodeFlaggingService : INavNodeFlaggingService
{
    /// <inheritdoc />
    public async Task UpdateHasContentFlagAsync(Dictionary<string, NavNode> tree, List<ISourceFile> files, CancellationToken cancellationToken = default)
    {
        foreach (var node in tree.Values)
        {
            if (node.Children != null)
                await UpdateHasContentFlagAsync(node.Children, files, cancellationToken);

            node.HasContent = files.Where(f => f.RelativePath.EndsWith(".md")).Any(f => f.Location == node.Location);
        }
    }
}