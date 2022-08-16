using System.Runtime.CompilerServices;
using Tomlyn;
using Tomlyn.Syntax;
using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models.Auth;

namespace TRENZ.Docs.API.Services;

public class TomlAuthzPermissionTableProvider : IPermissionTableProvider
{
    private readonly ISourcesProvider _sourcesProvider;
    private readonly ILogger<TomlAuthzPermissionTableProvider> _logger;

    public TomlAuthzPermissionTableProvider(
        ISourcesProvider sourcesProvider,
        ILogger<TomlAuthzPermissionTableProvider> logger
    )
    {
        _sourcesProvider = sourcesProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<PermissionTable> GetPermissionTablesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var authzFiles = _sourcesProvider.GetSources()
            .SelectMany(source => source.FindFiles(new("\\.authz")))
            .ToDictionary(
                sf => sf.RelativePath.Split(Path.DirectorySeparatorChar)[..^1],
                sf => sf
            )
            .OrderBy(kvp => kvp.Key.Length);

        foreach (var (filesystemPath, authzFile) in authzFiles)
        {
            var text = await authzFile.GetTextAsync(cancellationToken);
            var document = Toml.Parse(text, options: TomlParserOptions.ParseAndValidate);
            if (document.HasErrors)
            {
                _logger.LogError("Authz at \'{Path}\' contains syntax errors and is therefore ignored", authzFile.RelativePath);

                continue;
            }

            foreach (var table in ProcessDocument(filesystemPath, document))
            {
                yield return table;
            }
        }
    }

    public IEnumerable<PermissionTable> ProcessDocument(string[] filesystemPath, DocumentSyntax document)
    {
        _logger.LogDebug("Processing document at \'{Path}/.authz\'", string.Join("/", filesystemPath));

        foreach (var table in document.Tables)
        {
            var nodePath = filesystemPath.Append(table.Name!.ToString()).ToArray();
            var groups = new Dictionary<string, string[]>();
            foreach (var row in table.Items)
            {
                // FIXME: there has to be a better way instead of... this.
                var group = (row.Key!.Key! as BareKeySyntax)!.Key!.Text!;
                var permissions = (row.Value as ArraySyntax)!.Items.Select(s => (s.Value as StringValueSyntax)!.Value!.Trim()).ToArray();

                groups[group] = permissions;
            }

            yield return new(nodePath, groups);
        }
    }
}