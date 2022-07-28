using System.Text.RegularExpressions;

namespace TRENZ.Docs.API.Models.Sources;

public class MemorySource : ISource
{
    public MemorySource(string name, string root, string path, IEnumerable<MemorySourceFile> files)
    {
        Name = name;
        Root = root;
        Path = path;
        _files = files;
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public SourceType Type => SourceType.Memory;

    /// <inheritdoc />
    public string Root { get; }

    /// <inheritdoc />
    public string Path { get; }

    private readonly IEnumerable<MemorySourceFile> _files;

    /// <inheritdoc />
    public IEnumerable<ISourceFile> FindFiles(Regex pattern)
    {
        return _files.Where(sf => pattern.IsMatch(sf.RelativePath));
    }

    /// <inheritdoc />
    public Task UpdateAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}