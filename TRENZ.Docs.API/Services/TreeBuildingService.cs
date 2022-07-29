using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models;
using TRENZ.Docs.API.Models.Index;

namespace TRENZ.Docs.API.Services;

public class TreeBuildingService : ITreeBuildingService
{
    /// <inheritdoc />
    public Task<Dictionary<string, NavNode>> BuildTreeAsync(IEnumerable<IndexFile> indexFiles, CancellationToken cancellationToken = default)
    {
        var tree = new Dictionary<string, NavNode>();

        foreach (var file in indexFiles)
        {
            var path = file.location.Split(NavNode.Separator);
            var currentPath = new List<string>();
            var node = tree;
            for (var i = 0; i < path.Length; i++)
            {
                var part = path[i];
                currentPath.Add(part);

                if (!node.ContainsKey(part))
                {
                    node[part] = new(string.Join(NavNode.Separator, currentPath));
                }

                if (i + 1 < path.Length)
                {
                    node = node[part].Children ??= new();
                }
            }
        }

        return Task.FromResult(tree);
    }
}