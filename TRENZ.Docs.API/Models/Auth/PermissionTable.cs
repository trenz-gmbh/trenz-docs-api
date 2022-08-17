namespace TRENZ.Docs.API.Models.Auth;

public record PermissionTable(string[] LocationParts, Dictionary<string, string[]> Groups);
