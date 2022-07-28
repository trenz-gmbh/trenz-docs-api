using TRENZ.Docs.API.Models;

namespace TRENZ.Docs.API.Interfaces;

public interface INodeFlaggingService
{
    Task UpdateHasContentFlag(Dictionary<string, NavNode> tree);
}
