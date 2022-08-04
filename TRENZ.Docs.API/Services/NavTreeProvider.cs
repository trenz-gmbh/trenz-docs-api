using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models;
using TRENZ.Docs.API.Models.Index;

namespace TRENZ.Docs.API.Services;

public class NavTreeProvider : INavTreeProvider
{
    private readonly INavNodeFlaggingService _flaggingService;
    private readonly INavNodeOrderingService _orderingService;
    private readonly INavNodeAuthorizationService _authorizationService;

    public NavTreeProvider(INavNodeFlaggingService flaggingService, INavNodeOrderingService orderingService, INavNodeAuthorizationService authorizationService)
    {
        _flaggingService = flaggingService;
        _orderingService = orderingService;
        _authorizationService = authorizationService;
    }

    /// <inheritdoc />
    public async Task<NavTree> RebuildAsync(List<IndexFile> indexFiles, CancellationToken cancellationToken = default)
    {
        var root = new Dictionary<string, NavNode>();

        foreach (var file in indexFiles)
        {
            var path = file.location.Split(NavNode.Separator);
            var currentPath = new List<string>();
            var node = root;
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

        Tree = new(root);

        await PostProcessTree(indexFiles, cancellationToken);

        return Tree;
    }

    private async Task PostProcessTree(List<IndexFile> indexFiles, CancellationToken cancellationToken)
    {
        await _flaggingService.UpdateHasContentFlagAsync(Tree.Root, indexFiles, cancellationToken);
        await _orderingService.ReorderTreeAsync(Tree, cancellationToken);
        await _authorizationService.UpdateGroupsAsync(Tree, cancellationToken);
    }

    /// <inheritdoc />
    public NavTree Tree { get; private set; } = new(new());
}