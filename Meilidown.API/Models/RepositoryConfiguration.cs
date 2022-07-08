namespace Meilidown.API.Models;

public class RepositoryConfiguration
{
    private readonly IConfiguration _configuration;

    public string Name => _configuration["Name"];
    public string Url => _configuration["Url"];
    public string Branch => _configuration["Branch"] ?? "master";
    public string Path => _configuration["Path"];
    public string? Username => _configuration["Username"];
    public string? Password => _configuration["Password"];
    public string Root => System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Meilidown.Indexer", Name);

    public RepositoryConfiguration(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Repository: {{Name: {Name}, Url: {Url}, Branch: {Branch}, Path: {Path}}}";
    }
}