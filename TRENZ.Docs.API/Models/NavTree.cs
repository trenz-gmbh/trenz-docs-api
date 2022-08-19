namespace TRENZ.Docs.API.Models;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global
public class NavTree
{
    public NavTree(Dictionary<string, NavNode> root, bool containsUnauthorizedChildren = false)
    {
        Root = root;
        ContainsUnauthorizedChildren = containsUnauthorizedChildren;
    }

    public Dictionary<string, NavNode> Root { get; }

    public bool ContainsUnauthorizedChildren { get; }

    public NavTree WithoutHiddenNodes() => new(FilterChildrenBy(Root, node => node.Order >= 0), ContainsUnauthorizedChildren);

    public NavTree WithoutChildlessContentlessNodes() => new(FilterChildrenBy(Root, node => node.Children != null || node.HasContent), ContainsUnauthorizedChildren);

    public NavTree FilterByGroups(IEnumerable<string> groups)
    {
        var groupsList = groups.ToList();

        bool HasPermissionFor(NavNode node, params string[] permissions)
        {
            if (!node.Groups.Any())
                return true;

            return node.Groups.Keys
                .Intersect(groupsList)
                .Any(group => node.Groups[group].Intersect(permissions).Any());
        }

        var childExcluded = false;
        var newRoot = FilterChildrenBy(
            Root,
            node => HasPermissionFor(node, NavNode.PermissionList, NavNode.PermissionRead),
            node => HasPermissionFor(node, NavNode.PermissionRead),
            node =>
            {
                var include = HasPermissionFor(node, NavNode.PermissionList);
                if (!include)
                    node.ContainsUnauthorizedChildren = true;

                return include;
            },
            node => childExcluded = childExcluded || (!node.HasContent && node.Children == null)
        );

        return new(newRoot, childExcluded);
    }

    private static Dictionary<string, NavNode> FilterChildrenBy(
        Dictionary<string, NavNode> subtree,
        Func<NavNode, bool> includeNode,
        Func<NavNode, bool>? includeContent = null,
        Func<NavNode, bool>? includeChildren = null,
        Action<NavNode>? onNodeExcluded = null
    )
    {
        includeContent ??= _ => true;
        includeChildren ??= _ => true;

        return subtree
            .Select(kvp =>
            {
                var node = kvp.Value;
                if (node.Children == null)
                    return kvp;

                Dictionary<string, NavNode>? children = null;
                if (includeChildren(node))
                    children = FilterChildrenBy(node.Children, includeNode, includeContent, includeChildren, onNodeExcluded);

                var hasContent = node.HasContent;
                if (includeContent(node))
                {
                    hasContent = false;
                    onNodeExcluded?.Invoke(node);
                }

                var newNode = node.Clone();
                newNode.Children = children is { Count: > 0 } ? children : null;
                newNode.HasContent = hasContent;

                return new(
                    kvp.Key,
                    newNode
                );
            })
            .Where(kvp =>
            {
                var include = includeNode(kvp.Value);
                if (!include)
                    onNodeExcluded?.Invoke(kvp.Value);

                return include;
            })
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value
            );
    }

    public NavNode? FindNodeByLocation(string location) => InternalFindNodeByLocation(location, Root);

    public NavNode? FindNodeByLocationParts(string[] location) => InternalFindNodeByLocation(string.Join(NavNode.Separator, location), Root);

    private static NavNode? InternalFindNodeByLocation(string location, Dictionary<string, NavNode> subtree)
    {
        foreach (var node in subtree.Values)
        {
            if (node.Location.Equals(location, StringComparison.OrdinalIgnoreCase))
            {
                return node;
            }

            if (node.Children is not { Count: > 0 }) continue;
            var child = InternalFindNodeByLocation(location, node.Children);
            if (child != null)
            {
                return child;
            }
        }

        return null;
    }
}
