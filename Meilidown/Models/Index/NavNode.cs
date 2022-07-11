namespace Meilidown.Models.Index;

// ReSharper disable InconsistentNaming

public class NavNode
{
    public NavNode(string uid, string name, int order, string location, Dictionary<string, NavNode>? children = null)
    {
        this.uid = uid;
        this.name = name;
        this.order = order;
        this.location = location;
        this.children = children;
    }

    public string uid { get; init; }
    public string name { get; init; }
    public int order { get; init; }
    public string location { get; init; }
    public Dictionary<string, NavNode>? children { get; set; }

    public void Deconstruct(out string uid, out string name, out int order, out string location, out Dictionary<string, NavNode>? children)
    {
        uid = this.uid;
        name = this.name;
        order = this.order;
        location = this.location;
        children = this.children;
    }
}