using TRENZ.Docs.API.Models;
using TRENZ.Docs.API.Models.Index;

namespace TRENZ.Docs.API.Interfaces;

public interface INodeFlaggingService
{
    Task UpdateHasContentFlag(Dictionary<string, NavNode> tree, List<IndexFile> indexFiles);
}
