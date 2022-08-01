namespace TRENZ.Docs.API.Models.Index;

// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global

public record SearchResults(int totalHits, int processingTimeMs, IEnumerable<SearchResult> hits, int limit, int offset);
public record SearchResult(string name, string content, string location, SearchResultFormatted _formatted);
public record SearchResultFormatted(string name, string content, string location);