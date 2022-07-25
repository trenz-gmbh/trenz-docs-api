namespace TRENZ.Docs.API.Models.Index;

// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global

public record SearchResult(string name, string content, string location, SearchResultFormatted _formatted);
public record SearchResultFormatted(string name, string content, string location);