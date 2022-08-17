using System.Text;
using TRENZ.Docs.API.Models;
using TRENZ.Docs.API.Models.Sources;

namespace TRENZ.Docs.API.Test.Models.Sources;

public class MemorySourceFile : ISourceFile
{
    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string RelativePath { get; }
    
    /// <inheritdoc />
    public string Location { get; }

    private readonly string _contents;

    /// <summary>
    /// Creates an <see cref="ISourceFile"/> with the contents residing in memory
    /// </summary>
    /// <param name="name">The <see cref="RelativePath"/> converted to a display-friendly, normalized format (i.e. using
    /// <see cref="NavNode.PathToLocation"/>).</param>
    /// <param name="relativePath">The relative path from a <see cref="ISource"/> path to the file including file
    /// extensions. Contains platform specific separators.</param>
    /// <param name="contents">The contents to return when <see cref="GetBytesAsync"/>, <see cref="GetTextAsync"/> or
    /// <see cref="GetLinesAsync"/> is called.</param>
    /// <param name="location">Optionally override the location, this is normally derived from the
    /// <see cref="RelativePath"/> using <see cref="NavNode.PathToLocation"/></param>
    public MemorySourceFile(string relativePath, string contents, string? name = null, string? location = null)
    {
        RelativePath = relativePath;
        Location = location ?? NavNode.PathToLocation(string.Join('.', RelativePath.Split('.').SkipLast(1)));
        Name = name ?? Location.Split(NavNode.Separator).Last();
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