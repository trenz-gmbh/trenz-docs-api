using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models.Index;
using TRENZ.Docs.API.Models.Sources;

namespace TRENZ.Docs.API.Services
{
    /// <inheritdoc/>
    public class TreeOrderService : ITreeOrderService
    {
        private readonly ISourcesProvider _sourcesProvider;
        private readonly ILogger<TreeOrderService> _logger;

        public TreeOrderService(ISourcesProvider sourcesProvider,
                                ILogger<TreeOrderService> logger)
        {
            _sourcesProvider = sourcesProvider;
            _logger = logger;
        }

        public async Task ReorderTree(Dictionary<string, NavNode> tree)
        {
            var orderFiles = _sourcesProvider.GetSources()
                                             .SelectMany(source => source.FindFiles(new("\\.order")))
                                             .ToDictionary(sf => sf.RelativePath.Split(Path.DirectorySeparatorChar)[0..^1],
                                                           sf => sf);

            await SetOrder(tree, orderFiles);
        }

        private async Task SetOrder(Dictionary<string, NavNode> tree, Dictionary<string[], SourceFile> orderFiles)
        {
            int i = 0;

            foreach (var kvp in tree.OrderBy(x => x.Key))
            {
                var node = kvp.Value;

                await SetOrderByParent(node, orderFiles, i++);
            }

            await SetChildrenOrderByOrderFile(new string[] { }, tree.Values, orderFiles);
        }

        private async Task SetOrderByParent(NavNode node, Dictionary<string[], SourceFile> orderFiles, int index)
        {
            node.order = index;

            if (node.children == null)
                return;

            int childIndex = 0;

            // recurse all children
            foreach (var treeNode in node.children.OrderBy(x => x.Key))
                await SetOrderByParent(treeNode.Value, orderFiles, childIndex++);

            await SetChildrenOrderByOrderFile(node.location.Split('/'), node.children.Values, orderFiles);
        }

        private async Task SetChildrenOrderByOrderFile(string[] path,
                                                       IEnumerable<NavNode> children,
                                                       Dictionary<string[], SourceFile> orderFiles)
        {
            if (children == null)
                return;

            // if this particular folder has a .order, override the order
            var orderFile = orderFiles.SingleOrDefault(of => of.Key.SequenceEqual(path));

            if (orderFile.Value == null)
                return;

            var lines = await File.ReadAllLinesAsync(orderFile.Value.AbsolutePath);

            foreach (var item in children)
            {
                int newIndex = Array.IndexOf(lines, item.name);
                item.order = newIndex;

                if (newIndex < 0)
                    _logger.LogDebug($"Hiding {item.location}, according to `.order`");
                else
                    _logger.LogDebug($"Moving {item.location} to {newIndex}, according to `.order`");
            }
        }
    }
}

