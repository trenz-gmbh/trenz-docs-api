using TRENZ.Docs.API.Models;

namespace TRENZ.Docs.API.Interfaces;

public interface INavNodeAuthorizationService
{
    Task UpdateGroupsAsync(NavTree tree, CancellationToken cancellationToken = default);
}