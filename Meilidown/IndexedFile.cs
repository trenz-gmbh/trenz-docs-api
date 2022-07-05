namespace Meilidown;

public record IndexedFile(string Uid, string? ParentUid, string Content, int Order);