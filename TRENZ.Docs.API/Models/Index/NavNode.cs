﻿using System.Text.Json.Serialization;

namespace TRENZ.Docs.API.Models.Index;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

public class NavNode
{
    public const char Separator = '/';

    public NavNode(string uid, string location, bool hasContent = false, Dictionary<string, NavNode>? children = null)
    {
        Uid = uid;
        Location = location;
        HasContent = hasContent;
        Children = children;
    }

    public string Uid { get; }

    public int Order { get; set; }

    public string Location { get; }

    public bool HasContent { get; set; }

    public string NodeName => Location.Split(Separator).Last();

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, NavNode>? Children { get; set; }
}
