using System.Security.Cryptography;
using System.Text;

namespace TRENZ.Docs.API.Models.Sources;

public class PhysicalSourceFile : ISourceFile
{
    private const char UidSeparator = '#';
    private static string ToMd5(string text) => Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(text)));

    public PhysicalSourceFile(ISource source, string relativePath)
    {
        Source = source;
        RelativePath = relativePath;
        Uid = ToMd5(Source.Name + UidSeparator + RelativePath);
        Location = NavNode.PathToLocation(string.Join('.', RelativePath.Split('.').SkipLast(1)));
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
    public string Uid { get; }

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
