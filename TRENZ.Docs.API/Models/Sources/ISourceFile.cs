namespace TRENZ.Docs.API.Models.Sources;

public interface ISourceFile
{
    public string Uid { get; }
    public string Name { get; }
    public string Location { get; }
    public string RelativePath { get; }

    public Task<byte[]> GetBytesAsync(CancellationToken cancellationToken = default);
    
    public Task<string> GetTextAsync(CancellationToken cancellationToken = default);
    
    public Task<string[]> GetLinesAsync(CancellationToken cancellationToken = default);
}