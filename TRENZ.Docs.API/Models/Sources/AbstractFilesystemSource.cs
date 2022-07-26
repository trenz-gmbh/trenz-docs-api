using System.Text.RegularExpressions;
using IOPath = System.IO.Path;

namespace TRENZ.Docs.API.Models.Sources;

public abstract class AbstractFilesystemSource : ISource
{
    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public abstract SourceType Type { get; }

    /// <inheritdoc />
    public abstract string Root { get; }

    /// <inheritdoc />
    public abstract string Path { get; }

    /// <inheritdoc />
    public IEnumerable<SourceFile> FindFiles(Regex pattern)
    {
        var root = IOPath.Combine(Root, Path);
        return IterateDirectory(pattern, root, root);
    }

    /// <inheritdoc />
    public abstract Task UpdateAsync(CancellationToken cancellationToken = default);

    private IEnumerable<SourceFile> IterateDirectory(Regex pattern, string path, string root)
    {
        var info = new DirectoryInfo(path);
        var fileInfoEnumerable = info.EnumerateFiles("**", new EnumerationOptions
        {
            AttributesToSkip = FileAttributes.System,
            IgnoreInaccessible = true,
            MatchCasing = MatchCasing.CaseInsensitive,
            MatchType = MatchType.Win32,
            RecurseSubdirectories = true,
        });

        return from file in fileInfoEnumerable
            where file.Exists && pattern.IsMatch(file.Name)
            select new SourceFile(this, IOPath.GetRelativePath(root, file.FullName));
    }
}