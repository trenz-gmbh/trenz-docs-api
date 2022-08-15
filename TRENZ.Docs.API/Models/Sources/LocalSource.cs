namespace TRENZ.Docs.API.Models.Sources;

public sealed class LocalSource : AbstractFilesystemSource
{
    public static LocalSource FromConfiguration(IConfiguration configuration)
    {
        if (!string.Equals(configuration["Type"], SourceType.Local.GetValue(), StringComparison.InvariantCultureIgnoreCase))
        {
            throw new ArgumentException("Source type is not local");
        }

        if (string.IsNullOrEmpty(configuration["Name"]))
        {
            throw new ArgumentException("Name is required");
        }
        
        if (string.IsNullOrEmpty(configuration["Root"]))
        {
            throw new ArgumentException("Root is required");
        }

        return new(
            configuration["Name"],
            configuration["Root"],
            configuration["Path"] ?? ""
        );
    }

    public override SourceType Type => SourceType.Local;
    public override string Name { get; }
    public override string Root { get; }
    public override string Path { get; }

    public LocalSource(string name, string root, string path = "")
    {
        Name = name;
        Root = root;
        Path = path;
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