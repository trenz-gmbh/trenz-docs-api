namespace Meilidown.Models.Sources;

public class LocalSource : AbstractFilesystemSource
{
    private readonly IConfiguration _configuration;

    public override SourceType Type => SourceType.Local;
    public override string Name => _configuration["Name"];
    public override string Root => _configuration["Root"] ?? Path;
    public override string Path => _configuration["Path"];

    public LocalSource(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Local Source: {{Name: {Name}, Root: {Root}, Path: {Path}}}";
    }
}