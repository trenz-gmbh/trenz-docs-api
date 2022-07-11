using LibGit2Sharp;

namespace Meilidown.Models.Sources;

public class GitSource : AbstractFilesystemSource
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GitSource> _logger;

    public override SourceType Type => SourceType.Git;
    public override string Name => _configuration["Name"];
    public override string Root => System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Meilidown", Name);
    public override string Path => _configuration["Path"];

    public string Url => _configuration["Url"];
    public string Branch => _configuration["Branch"] ?? "master";
    public string? Username => _configuration["Username"];
    public string? Password => _configuration["Password"];

    public GitSource(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        _configuration = configuration;
        _logger = loggerFactory.CreateLogger<GitSource>();
    }
    
    /// <inheritdoc />
    public override async Task UpdateAsync(CancellationToken cancellationToken = default)
    {
        Credentials CredentialsHelper(string path, string username, SupportedCredentialTypes supportedCredentialTypes) =>
            new UsernamePasswordCredentials { Username = Username, Password = Password };

        var temp = Root;
        if (!Directory.Exists(temp))
        {
            _logger.LogInformation("Directory '{Temp}' does not exist. Cloning repository {SourceUrl}", temp, Url);
            Repository.Clone(Url, temp, new()
                {
                    BranchName = Branch,
                    CredentialsProvider = CredentialsHelper,
                }
            );
        }

        cancellationToken.ThrowIfCancellationRequested();

        using var repo = new Repository(temp);
        foreach (var remote in repo.Network.Remotes)
        {
            _logger.LogInformation("Fetching remote {Remote}", remote.Name);
            var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
            Commands.Fetch(repo, remote.Name, refSpecs, new()
            {
                CredentialsProvider = CredentialsHelper,
            }, "");

            cancellationToken.ThrowIfCancellationRequested();
        }

        if (repo.Head.FriendlyName != Branch)
        {
            _logger.LogInformation("Checking out branch {Branch}", Branch);
            Commands.Checkout(repo, Branch);
        }

        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation("Pulling changes for branch {Branch} of {SourceUrl}", Branch, Url);
        Commands.Pull(
            repo,
            new("Meilidown", "meilidown@example.com", DateTimeOffset.Now),
            new()
            {
                MergeOptions = new()
                {
                    MergeFileFavor = MergeFileFavor.Theirs,
                    FileConflictStrategy = CheckoutFileConflictStrategy.Theirs,
                },
                FetchOptions = new()
                {
                    CredentialsProvider = CredentialsHelper,
                },
            }
        );
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Git Source: {{Name: {Name}, Root: {Root}, Path: {Path}, Url: {Url}, Branch: {Branch}, Username: {Username}}}";
    }
}