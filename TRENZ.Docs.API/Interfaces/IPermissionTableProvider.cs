using TRENZ.Docs.API.Models.Auth;

namespace TRENZ.Docs.API.Interfaces;

public interface IPermissionTableProvider
{
    IAsyncEnumerable<PermissionTable> GetPermissionTablesAsync(CancellationToken cancellationToken = default);
}