using TRENZ.Docs.API.Models.Index;
using TRENZ.Docs.API.Models.Sources;

namespace TRENZ.Docs.API.Interfaces;

public interface IFileProcessingService
{
    IAsyncEnumerable<IndexFile> ProcessAsync(IEnumerable<ISourceFile> files, CancellationToken cancellationToken = default);
}