namespace TRENZ.Docs.API.Models.Index;

public record IndexStats(DateTimeOffset LastUpdate, int NumberOfDocuments, bool IsIndexing);
