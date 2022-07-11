using LibGit2Sharp;
using Meilidown.Interfaces;
using Meilidown.Models.Sources;

namespace Meilidown.Services;

public class GitSourceService : ISourceService<GitSource>
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GitSourceService> _logger;

    public GitSourceService(IConfiguration configuration, ILogger<GitSourceService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc />
    public IEnumerable<GitSource> GetSources()
    {
        return _configuration
            .GetSection("Sources")
            .GetChildren()
            .Where(s => s["Type"] != null)
            .Where(s => string.Equals(s["Type"], SourceType.Git.GetValue(), StringComparison.InvariantCultureIgnoreCase))
            .Select(s => new GitSource(s));
    }

    /// <inheritdoc />
    public async Task UpdateAsync(GitSource source, CancellationToken cancellationToken = default)
    {
        Credentials CredentialsHelper(string path, string username, SupportedCredentialTypes supportedCredentialTypes) =>
            new UsernamePasswordCredentials { Username = source.Username, Password = source.Password };

        var temp = source.Root;
        if (!Directory.Exists(temp))
        {
            _logger.LogInformation("Directory '{Temp}' does not exist. Cloning repository {SourceUrl}", temp, source.Url);
            Repository.Clone(source.Url, temp, new()
                {
                    BranchName = source.Branch,
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

        if (repo.Head.FriendlyName != source.Branch)
        {
            _logger.LogInformation("Checking out branch {Branch}", source.Branch);
            Commands.Checkout(repo, source.Branch);
        }

        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation("Pulling changes for branch {Branch} of {SourceUrl}", source.Branch, source.Url);
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
}