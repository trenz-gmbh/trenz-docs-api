using System.Text.Json.Serialization;

namespace TRENZ.Docs.API.Models;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global
public class NavNode
{
    /// <summary>
    /// The separator used to represent the hierarchy in the location and path.
    /// </summary>
    public const char Separator = '/';

    /// <summary>
    /// The permission to read the content of a node.
    /// </summary>
    public const string PermissionRead = "read";

    /// <summary>
    /// The permission to list the children of a node.
    /// </summary>
    public const string PermissionList = "list";

    /// <summary>
    /// Represents a node within a navigation tree. Can have children.
    /// </summary>
    /// <param name="location">The location of the node in the tree.</param>
    public NavNode(string location)
    {
        Location = location;
    }

    /// <summary>
    /// The order relative to other nodes on the same level. The first node is 0. Nodes with Order less than 0 are
    /// hidden.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// The location of the node in the tree. This is optimized for display in the browser.
    /// </summary>
    public string Location { get; }

    /// <summary>
    /// The path of the node in the tree (<see cref="Location" />) split up by the separator.
    /// Is ignored during JSON serialization.
    /// </summary>
    [JsonIgnore]
    public IEnumerable<string> LocationParts => Location.Split(Separator);

    /// <summary>
    /// Whether or not this node has a corresponding content file.
    /// </summary>
    public bool HasContent { get; set; }

    /// <summary>
    /// Whether or not this node contains children which were filtered out due to permissions.
    /// </summary>
    public bool HasHiddenChildren { get; set; }

    /// <summary>
    /// The display name of the node in the tree. This is the last part of <see cref="Location" />.
    /// </summary>
    public string NodeName => LocationParts.Last();

    /// <summary>
    /// The filename of the corresponding content file without extension.
    /// Is ignored during JSON serialization.
    /// </summary>
    [JsonIgnore]
    public string FileName => LocationToPath(NodeName);

    /// <summary>
    /// Optionally contains a list of child nodes, keyed by their <see cref="NodeName" />.
    /// Is excluded from the JSON object if <c>null</c>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, NavNode>? Children { get; set; }

    /// <summary>
    /// Contains a dictionary keyed by the group name whose values is an array of permissions of this group for this
    /// node. Child nodes have their own set of group permissions.
    /// Is ignored during JSON serialization.
    /// </summary>
    [JsonIgnore]
    public Dictionary<string, string[]> Groups { get; set; } = new();

    /// <summary>
    /// Creates a new instance of a <see cref="NavNode" /> with the same properties as this one.
    /// Also clones all child nodes recursively.
    /// </summary>
    /// <returns>A new nav node without any references to this node.</returns>
    public NavNode Clone()
    {
        return new(Location)
        {
            Groups = Groups,
            Order = Order,
            HasContent = HasContent,
            HasHiddenChildren = HasHiddenChildren,
            Children = Children?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Clone())
        };
    }

    /// <summary>
    /// Converts a physical file path to a location in the tree.
    /// </summary>
    /// <param name="path">The path to a physical file.</param>
    /// <returns>A string with the corresponding location in the tree.</returns>
    public static string PathToLocation(string path)
    {
        return path
            .Replace(Path.DirectorySeparatorChar, Separator)
            .Replace('-', ' ')
            .Replace("%2D", "-");
    }

    /// <summary>
    /// Converts a location in the tree to a physical file path.
    /// </summary>
    /// <param name="location">The location of a node in the tree.</param>
    /// <returns>A string corresponding to the physical location of a file.</returns>
    public static string LocationToPath(string location)
    {
        return location
            .Replace(Separator, Path.DirectorySeparatorChar)
            .Replace("-", "%2D")
            .Replace(' ', '-');
    }
}