namespace Meilidown.Models.Sources;

public class GitSource : AbstractFilesystemSource
{
    private readonly IConfiguration _configuration;

    public override SourceType Type => SourceType.Git;
    public override string Name => _configuration["Name"];
    public override string Root => System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Meilidown", Name);
    public override string Path => _configuration["Path"];

    public string Url => _configuration["Url"];
    public string Branch => _configuration["Branch"] ?? "master";
    public string? Username => _configuration["Username"];
    public string? Password => _configuration["Password"];

    public GitSource(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Git Source: {{Name: {Name}, Root: {Root}, Path: {Path}, Url: {Url}, Branch: {Branch}, Username: {Username}}}";
    }
}