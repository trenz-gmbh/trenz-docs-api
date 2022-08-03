namespace TRENZ.Docs.API.Models;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global
public class NavTree
{
    public NavTree(Dictionary<string, NavNode> root)
    {
        Root = root;
    }

    public Dictionary<string, NavNode> Root { get; set; }

    public NavTree WithoutHiddenNodes() => new(FilterChildrenBy(Root, kvp => kvp.Value.Order >= 0));

    public NavTree FilterByGroups(IEnumerable<string> groups) => new(FilterChildrenBy(Root, kvp => !kvp.Value.Groups.Any() || kvp.Value.Groups.Keys.Intersect(groups).Any())); // TODO check if children listing is allowed

    private static Dictionary<string, NavNode> FilterChildrenBy(Dictionary<string, NavNode> subtree,  Func<KeyValuePair<string, NavNode>, bool> predicate)
    {
        return subtree
            .Where(predicate)
            .Select(kvp =>
            {
                if (kvp.Value.Children != null)
                    return new(
                        kvp.Key,
                        new(
                            kvp.Value.Location,
                            kvp.Value.HasContent,
                            FilterChildrenBy(kvp.Value.Children, predicate)
                        )
                    );

                return kvp;
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
