using TRENZ.Docs.API.Models.Index;

namespace TRENZ.Docs.API.Interfaces;

public interface IIndexingService
{
    Task IndexAsync(List<IndexFile> files, CancellationToken cancellationToken = default);
    Task<IndexFile?> GetIndexedFile(string location, CancellationToken cancellationToken = default);
    Task<IEnumerable<SearchResult>> Search(string query, CancellationToken cancellationToken = default);
    Task<IndexStats> GetStats(CancellationToken cancellationToken = default);
}