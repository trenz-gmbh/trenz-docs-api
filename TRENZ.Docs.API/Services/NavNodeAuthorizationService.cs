using Tomlyn;
using Tomlyn.Syntax;
using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models;

namespace TRENZ.Docs.API.Services;

public class NavNodeAuthorizationService : INavNodeAuthorizationService
{
    private readonly ISourcesProvider _sourcesProvider;
    private readonly ILogger<NavNodeAuthorizationService> _logger;

    public NavNodeAuthorizationService(
        ISourcesProvider sourcesProvider,
        ILogger<NavNodeAuthorizationService> logger
    )
    {
        _sourcesProvider = sourcesProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task UpdateGroupsAsync(NavTree tree, CancellationToken cancellationToken = default)
    {
        var authzFiles = _sourcesProvider.GetSources()
            .SelectMany(source => source.FindFiles(new("\\.authz")))
            .ToDictionary(
                sf => sf.RelativePath.Split(Path.DirectorySeparatorChar)[..^1],
                sf => sf
            )
            .OrderBy(kvp => kvp.Key.Length);

        foreach (var (path, authzFile) in authzFiles)
        {
            var text = await authzFile.GetTextAsync(cancellationToken);
            var parsed = Toml.Parse(text, options: TomlParserOptions.ParseAndValidate);

            if (parsed.HasErrors)
            {
                _logger.LogError($"Authz at '{authzFile.RelativePath}' contains syntax errors and is therefore ignored.");

                continue;
            }

            _logger.LogDebug($"Processing authz file at '{authzFile.RelativePath}'.");

            foreach (var table in parsed.Tables)
            {
                var nodePath = path.Append(table.Name!.ToString()).ToArray();
                var node = tree.FindNodeByLocationParts(nodePath);
                if (node == null)
                {
                    _logger.LogWarning($"Authz at '{authzFile.RelativePath}' contains a table for a node at '{string.Join("/", nodePath)}' which does not exist.");

                    continue;
                }

                foreach (var item in table.Items)
                {
                    // FIXME: there has to be a better way instead of... this.
                    var group = (item.Key!.Key! as BareKeySyntax)!.Key!.Text!;
                    var permissions = (item.Value as ArraySyntax)!.Items.Select(s => (s.Value as StringValueSyntax)!.Value!.Trim());

                    await SetGroupsRecursivelyAsync(node, group, permissions.ToArray(), cancellationToken);
                }
            }
        }
    }

    private async Task SetGroupsRecursivelyAsync(NavNode node, string group, string[] permissions, CancellationToken cancellationToken = default)
    {
        // FIXME: need a better way to negotiate which set of permissions to use. For now, we'll just use the set with _less_ permissions (the more restrictive one).
        if (!node.Groups.ContainsKey(group))
        {
            node.Groups[group] = permissions;

            _logger.LogDebug($"Group {group} has permissions [{string.Join(", ", permissions)}] for node {node.Location}.");
        }
        else if (node.Groups[group].Length > permissions.Length)
        {
            var previous = node.Groups[group];
            node.Groups[group] = permissions;

            _logger.LogDebug($"Updated permissions for group {group} to [{string.Join(",", permissions)}] for node {node.Location} (was [{string.Join(",", previous)}]).");
        }
        else
        {
            _logger.LogDebug($"Skipping updating permissions for group {group} for node {node.Location} because the current permissions are already more restrictive ({string.Join(",", node.Groups[group])} <=> {string.Join(",", permissions)}).");
        }

        if (node.Children == null)
            return;

        foreach (var child in node.Children)
        {
            await SetGroupsRecursivelyAsync(child.Value, group, permissions, cancellationToken);
        }
    }
}