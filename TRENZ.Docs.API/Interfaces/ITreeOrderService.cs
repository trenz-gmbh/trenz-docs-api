using TRENZ.Docs.API.Models.Index;

namespace TRENZ.Docs.API.Interfaces
{
    /// <summary>
    /// <para>Gives children within a <c>NavNode</c> (same parent) a defined order.</para>
    ///
    /// <para>The behavior matches https://docs.microsoft.com/en-us/azure/devops/project/wiki/wiki-file-structure.
    /// If a <c>.order</c> file exists, it defines the canonical order. If it doesn't,
    /// nodes are ordered alphabetically.</para>
    /// </summary>
    public interface ITreeOrderService
    {
        Task ReorderTree(Dictionary<string, NavNode> tree);
    }
}
