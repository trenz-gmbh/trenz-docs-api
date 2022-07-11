namespace Meilidown.Models;

// ReSharper disable InconsistentNaming

public record IndexedFile(string uid, string name, string content, int order, string location);