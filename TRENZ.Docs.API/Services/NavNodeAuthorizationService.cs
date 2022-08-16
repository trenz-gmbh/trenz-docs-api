using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models;

namespace TRENZ.Docs.API.Services;

public class NavNodeAuthorizationService : INavNodeAuthorizationService
{
    private readonly IPermissionTableProvider _permissionTableProvider;
    private readonly ILogger<NavNodeAuthorizationService> _logger;

    public NavNodeAuthorizationService(
        IPermissionTableProvider permissionTableProvider,
        ILogger<NavNodeAuthorizationService> logger
    )
    {
        _permissionTableProvider = permissionTableProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task UpdateGroupsAsync(NavTree tree, CancellationToken cancellationToken = default)
    {
        await foreach (var table in _permissionTableProvider.GetPermissionTablesAsync(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var node = tree.FindNodeByLocationParts(table.LocationParts);
            if (node == null)
            {
                _logger.LogWarning("Authz at '{Location}/.authz' contains a table for a node at '{Location}' which does not exist.", string.Join("/", table.LocationParts));

                continue;
            }

            foreach (var row in table.Groups)
            {
                await SetGroupsRecursivelyAsync(node, row.Key, row.Value, cancellationToken);
            }
        }
    }

    private async Task SetGroupsRecursivelyAsync(NavNode node, string group, string[] permissions, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // FIXME: need a better way to negotiate which set of permissions to use. For now, we'll just use the set with _less_ permissions (the more restrictive one).
        if (!node.Groups.ContainsKey(group))
        {
            node.Groups[group] = permissions;

            _logger.LogDebug("Group {Group} has permissions [{Permissions}] for node '{Location}'", group, string.Join(", ", permissions), node.Location);
        }
        else if (node.Groups[group].Length >= permissions.Length)
        {
            var previous = node.Groups[group];
            node.Groups[group] = permissions;

            _logger.LogDebug("Updated permissions for group {Group} to [{Permissions}] for node '{Location}' (was [{OldPermissions}])", group, string.Join(",", permissions), node.Location, string.Join(",", previous));
        }
        else
        {
            _logger.LogDebug("Skipping updating permissions for group {Group} for node '{Location}' because the current permissions are already more restrictive ([{Permissions}] <=> [{NewPermissions}])", group, node.Location, string.Join(",", node.Groups[group]), string.Join(",", permissions));
        }

        if (node.Children == null)
            return;

        foreach (var child in node.Children)
        {
            await SetGroupsRecursivelyAsync(child.Value, group, permissions, cancellationToken);
        }
    }
}