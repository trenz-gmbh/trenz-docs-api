using Tomlyn;
using Tomlyn.Model;
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
            );

        foreach (var (path, authzFile) in authzFiles)
        {
            var text = await authzFile.GetTextAsync(cancellationToken);
            var parsed = Toml.Parse(text, options: TomlParserOptions.ParseAndValidate);

            if (parsed.HasErrors)
            {
                _logger.LogError($"Authz at '{authzFile.RelativePath}' contains syntax errors and is therefore ignored.");

                continue;
            }

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
                    var group = item.Key!.ToString();
                    var permissions = item.Value;
                }
            }
        }

        // await UpdateChildrenGroupsAsync(new(), tree.Root, authzFiles, cancellationToken);
    }

    // private async Task UpdateChildrenGroupsAsync(List<string> previousParts, Dictionary<string, NavNode> subtree, Dictionary<string[], ISourceFile> authzFiles, CancellationToken cancellationToken = default)
    // {
    //     foreach (var (key, node) in subtree)
    //     {
    //         var pathParts = previousParts.Append(key).ToList();
    //
    //         _logger.LogInformation("Visited node {Path}", string.Join('/', pathParts));
    //
    //         if (node.Children != null)
    //         {
    //             await UpdateChildrenGroupsAsync(pathParts, node.Children, authzFiles, cancellationToken);
    //         }
    //     }
    // }
}