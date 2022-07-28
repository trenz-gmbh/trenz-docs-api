using System.Text.RegularExpressions;

namespace TRENZ.Docs.API.Models.Sources;

public interface ISource
{
    public string Name { get; }

    public SourceType Type { get; }

    public string Root { get; }

    public string Path { get; }

    public IEnumerable<ISourceFile> FindFiles(Regex pattern);

    Task UpdateAsync(CancellationToken cancellationToken = default);
}