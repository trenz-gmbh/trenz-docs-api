using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models;
using TRENZ.Docs.API.Models.Sources;

namespace TRENZ.Docs.API.Services
{
    /// <inheritdoc/>
    public class NavNodeOrderingService : INavNodeOrderingService
    {
        private readonly ISourcesProvider _sourcesProvider;
        private readonly ILogger<NavNodeOrderingService> _logger;

        public NavNodeOrderingService(ISourcesProvider sourcesProvider,
                                ILogger<NavNodeOrderingService> logger)
        {
            _sourcesProvider = sourcesProvider;
            _logger = logger;
        }

        public async Task ReorderTreeAsync(NavTree tree, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Reordering tree...");

            var orderFiles = _sourcesProvider.GetSources()
                                             .SelectMany(source => source.FindFiles(new("\\.order")))
                                             .ToDictionary(sf => NavNode.PathToLocation(sf.RelativePath).Split(NavNode.Separator)[..^1],
                                                           sf => sf);

            var i = 0;
            foreach (var (_, node) in tree.Root.OrderBy(x => x.Key))
            {
                await SetOrderByParent(node, orderFiles, i++, cancellationToken);
            }

            await SetChildrenOrderByOrderFile(Array.Empty<string>(), tree.Root.Values, orderFiles, cancellationToken);

            _logger.LogDebug("Reordering done");
        }

        private async Task SetOrderByParent(NavNode node, Dictionary<string[], ISourceFile> orderFiles, int index, CancellationToken cancellationToken)
        {
            node.Order = index;

            if (node.Children == null)
                return;

            int childIndex = 0;

            // recurse all children
            foreach (var treeNode in node.Children.OrderBy(x => x.Key))
                await SetOrderByParent(treeNode.Value, orderFiles, childIndex++, cancellationToken);

            await SetChildrenOrderByOrderFile(node.LocationParts, node.Children.Values, orderFiles, cancellationToken);
        }

        private async Task SetChildrenOrderByOrderFile(IEnumerable<string> locationParts,
                                                       IEnumerable<NavNode> children,
                                                       Dictionary<string[], ISourceFile> orderFiles,
                                                       CancellationToken cancellationToken = default)
        {
            if (children == null)
                return;

            // if this particular folder has a .order, override the order
            var orderFile = orderFiles.SingleOrDefault(of => of.Key.SequenceEqual(locationParts));

            if (orderFile.Value == null)
                return;

            var lines = await orderFile.Value.GetLinesAsync(cancellationToken);

            foreach (var item in children)
            {
                var newIndex = Array.IndexOf(lines, item.FileName);
                item.Order = newIndex;

                _logger.LogDebug(newIndex < 0 ? $"Hiding {item.Location}, according to `.order`" : $"Moving {item.Location} to {newIndex}, according to `.order`");
            }
        }
    }
}
