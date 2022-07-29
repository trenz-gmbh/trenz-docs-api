using TRENZ.Docs.API.Models;

namespace TRENZ.Docs.API.Test;

internal class TreeNodeEqualityComparer : IEqualityComparer<KeyValuePair<string, NavNode>>
{
    /// <inheritdoc />
    bool IEqualityComparer<KeyValuePair<string, NavNode>>.Equals(KeyValuePair<string, NavNode> x, KeyValuePair<string, NavNode> y)
    {
        if (x.Key != y.Key)
            return false;

        if (x.Value.Location != y.Value.Location)
            return false;

        if (x.Value.Order != y.Value.Order)
            return false;

        return x.Value.HasContent == y.Value.HasContent;
    }

    /// <inheritdoc />
    public int GetHashCode(KeyValuePair<string, NavNode> obj)
    {
        return obj.GetHashCode();
    }
}