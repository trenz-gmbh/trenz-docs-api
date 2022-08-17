using System.Text;
using TRENZ.Docs.API.Models;
using TRENZ.Docs.API.Models.Sources;

namespace TRENZ.Docs.API.Test.Models.Sources;

public class MemorySourceFile : ISourceFile
{
    /// <inheritdoc />
    public string Uid { get; }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string RelativePath { get; }
    
    /// <inheritdoc />
    public string Location { get; }

    private readonly string _contents;
    
    public MemorySourceFile(string uid, string name, string relativePath, string contents, string? location = null)
    {
        Uid = uid;
        Name = name;
        Location = location ?? NavNode.PathToLocation(relativePath);
        RelativePath = relativePath;
        _contents = contents;
    }

    /// <inheritdoc />
    public Task<byte[]> GetBytesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Encoding.UTF8.GetBytes(_contents));
    }

    /// <inheritdoc />
    public Task<string> GetTextAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_contents);
    }

    /// <inheritdoc />
    public Task<string[]> GetLinesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_contents.ReplaceLineEndings().Split(Environment.NewLine));
    }
}