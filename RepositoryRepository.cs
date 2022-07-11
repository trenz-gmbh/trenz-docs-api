using System.Text.RegularExpressions;
using LibGit2Sharp;
using Meilidown.Models;

namespace Meilidown;

public static class RepositoryRepository
{
    public static IEnumerable<ISourceConfiguration> GetRepositories(IConfiguration configuration, string key = "Sources")
    {
        return configuration.GetRequiredSection(key).GetChildren().Select(source => new GitSourceConfiguration(source));
    }

    public static void Update(this GitSourceConfiguration config)
    {
        Credentials CredentialsHelper(string path, string username, SupportedCredentialTypes supportedCredentialTypes) =>
            new UsernamePasswordCredentials { Username = config.Username, Password = config.Password };

        var temp = config.Root;
        if (!Directory.Exists(temp))
        {
            Repository.Clone(config.Url, temp, new()
                {
                    BranchName = config.Branch,
                    CredentialsProvider = CredentialsHelper,
                }
            );
        }

        using var repo = new Repository(temp);
        foreach (var remote in repo.Network.Remotes)
        {
            var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
            Commands.Fetch(repo, remote.Name, refSpecs, new()
            {
                CredentialsProvider = CredentialsHelper,
            }, $"Fetching remote {remote.Name}");
        }

        if (repo.Head.FriendlyName != config.Branch)
        {
            Commands.Checkout(repo, config.Branch);
        }

        Commands.Pull(
            repo,
            new("Meilidown.Indexer", "meilidown@example.com", DateTimeOffset.Now),
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

    public static IEnumerable<SourceFile> FindFiles(this GitSourceConfiguration config, Regex pattern)
    {
        var root = Path.Combine(config.Root, config.Path);
        return IterateDirectory(config, pattern, root, root);
    }

    private static IEnumerable<SourceFile> IterateDirectory(GitSourceConfiguration config, Regex pattern, string path, string root)
    {
        foreach (var file in Directory.EnumerateFileSystemEntries(path, "**", new EnumerationOptions
                 {
                     RecurseSubdirectories = true,
                     MatchType = MatchType.Win32,
                     IgnoreInaccessible = true,
                     MatchCasing = MatchCasing.CaseInsensitive,
                 }).Where(f => pattern.IsMatch(Path.GetFileName(f))))
        {
            if (File.Exists(file))
            {
                yield return new(config, Path.GetRelativePath(root, file));

                continue;
            }

            if (!Directory.Exists(file))
                continue;

            foreach (var f in IterateDirectory(config, pattern, file, root))
            {
                yield return f;
            }
        }
    }
}