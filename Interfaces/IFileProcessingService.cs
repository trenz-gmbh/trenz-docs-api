using Meilidown.Models.Index;
using Meilidown.Models.Sources;

namespace Meilidown.Interfaces;

public interface IFileProcessingService
{
    IAsyncEnumerable<IndexFile> ProcessAsync(IAsyncEnumerable<SourceFile> files, CancellationToken cancellationToken = default);
}