using Meilidown.Models.Index;

namespace Meilidown.Interfaces
{
    public interface ITreeOrderService
    {
        Task ReorderTree(Dictionary<string, NavNode> tree);
    }
}
