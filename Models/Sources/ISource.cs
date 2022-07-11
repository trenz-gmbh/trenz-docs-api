using System.Text.RegularExpressions;

namespace Meilidown.Models.Sources;

public interface ISource
{
    public string Name { get; }

    public SourceType Type { get; }

    public string Root { get; }

    public string Path { get; }

    public IEnumerable<SourceFile> FindFiles(Regex pattern);

    Task UpdateAsync(CancellationToken cancellationToken = default);
}