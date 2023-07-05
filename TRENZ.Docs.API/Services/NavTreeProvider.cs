using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models;
using TRENZ.Docs.API.Models.Sources;

namespace TRENZ.Docs.API.Services;

public class NavTreeProvider : INavTreeProvider
{
    private readonly ILogger<NavTreeProvider> _logger;
    private readonly INavNodeFlaggingService _flaggingService;
    private readonly INavNodeOrderingService _orderingService;
    private readonly INavNodeAuthorizationService _authorizationService;

    public NavTreeProvider(INavNodeFlaggingService flaggingService, INavNodeOrderingService orderingService, INavNodeAuthorizationService authorizationService, ILogger<NavTreeProvider> logger)
    {
        _flaggingService = flaggingService;
        _orderingService = orderingService;
        _authorizationService = authorizationService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<NavTree> RebuildAsync(List<ISourceFile> files, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Rebuilding tree using {Count} files", files.Count);

        var root = new Dictionary<string, NavNode>();

        foreach (var file in files)
        {
            var locationParts = file.Location.Split(NavNode.Separator);
            var currentLocation = new List<string>();
            var node = root;
            for (var i = 0; i < locationParts.Length; i++)
            {
                var nodeName = locationParts[i];
                if (nodeName.EndsWith(".md"))
                {
                    nodeName = nodeName[..^3];
                }

                currentLocation.Add(nodeName);

                if (!node.ContainsKey(nodeName))
                {
                    node[nodeName] = new(string.Join(NavNode.Separator, currentLocation));
                }

                if (i + 1 < locationParts.Length)
                {
                    node = node[nodeName].Children ??= new();
                }
            }

            _logger.LogTrace("Added file to tree: {Location}", string.Join(NavNode.Separator, currentLocation));
        }

        Tree = new(root);

        await PostProcessTree(files, cancellationToken);

        _logger.LogDebug("Tree rebuilt");

        return Tree;
    }

    private async Task PostProcessTree(List<ISourceFile> files, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Post processing tree...");

        await _flaggingService.UpdateHasContentFlagAsync(Tree.Root, files, cancellationToken);
        await _orderingService.ReorderTreeAsync(Tree, cancellationToken);
        await _authorizationService.UpdateGroupsAsync(Tree, cancellationToken);
    }

    /// <inheritdoc />
    public NavTree Tree { get; private set; } = new(new());
}