using Meilidown.Models.Index;

namespace Meilidown.Interfaces;

public interface IIndexingService
{
    Task IndexAsync(IEnumerable<IndexFile> files, CancellationToken cancellationToken = default);
    Task<IEnumerable<IndexFile>> GetIndexedFiles(CancellationToken cancellationToken = default);
    Task<IndexFile?> GetIndexedFile(string location, CancellationToken cancellationToken = default);
    Task<IEnumerable<SearchResult>> Search(string query, CancellationToken cancellationToken = default);
}