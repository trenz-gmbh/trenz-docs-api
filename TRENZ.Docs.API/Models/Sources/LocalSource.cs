namespace TRENZ.Docs.API.Models.Sources;

public sealed class LocalSource : AbstractFilesystemSource
{
    private readonly IConfiguration _configuration;

    public override SourceType Type => SourceType.Local;
    public override string Name => _configuration["Name"];
    public override string Root => _configuration["Root"];
    public override string Path => _configuration["Path"] ?? "";

    public LocalSource(IConfiguration configuration)
    {
        _configuration = configuration;

        if (!string.Equals(_configuration["Type"], SourceType.Local.GetValue(), StringComparison.InvariantCultureIgnoreCase))
        {
            throw new ArgumentException("Source type is not local");
        }

        if (string.IsNullOrEmpty(Name))
        {
            throw new ArgumentException("Name is required");
        }
        
        if (string.IsNullOrEmpty(Root))
        {
            throw new ArgumentException("Root is required");
        }
    }

    /// <inheritdoc />
    public override Task UpdateAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Local Source: {{Name: {Name}, Root: {Root}, Path: {Path}}}";
    }
}