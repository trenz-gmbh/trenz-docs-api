using TRENZ.Docs.API.Models;
using TRENZ.Docs.API.Models.Index;

namespace TRENZ.Docs.API.Interfaces;

public interface ITreeBuildingService
{
    Task<Dictionary<string, NavNode>> BuildTreeAsync(IEnumerable<IndexFile> indexFiles, CancellationToken cancellationToken = default);
}