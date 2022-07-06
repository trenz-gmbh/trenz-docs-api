namespace Meilidown;

public record IndexedFile(string Uid, string? ParentUid, string Name, string Content, int Order);