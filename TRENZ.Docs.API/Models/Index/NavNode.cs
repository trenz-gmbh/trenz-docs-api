using System.Text.Json.Serialization;

namespace TRENZ.Docs.API.Models.Index;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

public class NavNode
{
    public const char Separator = '/';

    public NavNode(string uid, string displayName, string location, Dictionary<string, NavNode>? children = null)
    {
        Uid = uid;
        DisplayName = displayName;
        Location = location;
        Children = children;
    }

    public string Uid { get; }
    public string DisplayName { get; }
    public int Order { get; set; }
    public string Location { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, NavNode>? Children { get; set; }

    [JsonIgnore]
    public string NodeName => Location.Split(Separator).Last();
}