using System.Security.Cryptography;
using System.Text;

namespace TRENZ.Docs.API.Models.Sources;

public class PhysicalSourceFile : ISourceFile
{
    public PhysicalSourceFile(ISource source, string relativePath)
    {
        Source = source;
        RelativePath = relativePath;
        Location = NavNode.PathToLocation(RelativePath.EndsWith(".md") ? RelativePath[..^3] : RelativePath);
        Name = Location.Split(NavNode.Separator).Last();
        AbsolutePhysicalPath = Path.Combine(Source.Root, Source.Path, RelativePath);
    }

    private ISource Source { get; }
    private string AbsolutePhysicalPath { get; }

    /// <inheritdoc />
    public string Location { get; }

    /// <inheritdoc />
    public string RelativePath { get; }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public async Task<byte[]> GetBytesAsync(CancellationToken cancellationToken = default) => await File.ReadAllBytesAsync(AbsolutePhysicalPath, cancellationToken);
 
    /// <inheritdoc />
    public async Task<string> GetTextAsync(CancellationToken cancellationToken = default) => await File.ReadAllTextAsync(AbsolutePhysicalPath, cancellationToken);

    /// <inheritdoc />
    public async Task<string[]> GetLinesAsync(CancellationToken cancellationToken = default) => await File.ReadAllLinesAsync(AbsolutePhysicalPath, cancellationToken);

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{nameof(PhysicalSourceFile)}: ({Source.Name}) {AbsolutePhysicalPath}";
    }
}
