using TRENZ.Docs.API.Models.Index;
using TRENZ.Docs.API.Models.Sources;

namespace TRENZ.Docs.API.Interfaces;

public interface IFileProcessingService
{
    IAsyncEnumerable<IndexFile> ProcessAsync(IAsyncEnumerable<SourceFile> files, CancellationToken cancellationToken = default);
}