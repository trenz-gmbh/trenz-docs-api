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
            node => HasPermissionFor(node, NavNode.PermissionList),
            () => childExcluded = true
        );

        return new(newRoot, childExcluded);
    }

    private static Dictionary<string, NavNode> FilterChildrenBy(
        Dictionary<string, NavNode> subtree,
        Func<NavNode, bool> includeNode,
        Func<NavNode, bool>? includeContent = null,
        Func<NavNode, bool>? includeChildren = null,
        Action? onNodeExcluded = null
    )
    {
        includeContent ??= _ => true;
        includeChildren ??= _ => true;

        return subtree
            .Where(kvp =>
            {
                var include = includeNode(kvp.Value);
                if (!include)
                    onNodeExcluded?.Invoke();

                return include;
            })
            .Select(kvp =>
            {
                if (kvp.Value.Children == null)
                    return kvp;

                Dictionary<string, NavNode>? children = null;
                var childExcluded = false;
                if (includeChildren(kvp.Value))
                    children = FilterChildrenBy(kvp.Value.Children, includeNode, includeContent, includeChildren, () => childExcluded = true);

                var hasContent = kvp.Value.HasContent;
                if (includeContent(kvp.Value))
                {
                    hasContent = false;
                    onNodeExcluded?.Invoke();
                }

                var newNode = kvp.Value.Clone();
                newNode.Children = children is { Count: > 0 } ? children : null;
                newNode.HasContent = hasContent;
                newNode.ContainsUnauthorizedChildren = childExcluded;

                return new(
                    kvp.Key,
                    newNode
                );
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
