using TRENZ.Docs.API.Models.Index;

namespace TRENZ.Docs.API.Interfaces;

public interface IIndexingService
{
    Task IndexAsync(IEnumerable<IndexFile> files, CancellationToken cancellationToken = default);
    Task<IEnumerable<IndexFile>> GetIndexedFiles(CancellationToken cancellationToken = default);
    Task<IndexFile?> GetIndexedFile(string location, CancellationToken cancellationToken = default);
    Task<IEnumerable<SearchResult>> Search(string query, CancellationToken cancellationToken = default);
    Task<IndexStats> GetStats(CancellationToken cancellationToken = default);
}