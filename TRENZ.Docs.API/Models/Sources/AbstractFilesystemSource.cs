using System.Security;
using System.Text.RegularExpressions;
using FSPath = System.IO.Path;

namespace TRENZ.Docs.API.Models.Sources;

public abstract class AbstractFilesystemSource : ISource
{
    private static SecurityException PreventedPathTraversal => new("Prevented an attempt to traverse a path outside the source's root.");

    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public abstract SourceType Type { get; }

    /// <inheritdoc />
    public abstract string Root { get; }

    /// <inheritdoc />
    public abstract string Path { get; }

    /// <inheritdoc />
    public IEnumerable<ISourceFile> FindFiles(Regex pattern)
    {
        var combinedRoot = FSPath.Combine(Root, Path);

        if (!FSPath.GetFullPath(combinedRoot).StartsWith(Root))
            throw PreventedPathTraversal;

        return IterateDirectory(pattern, combinedRoot, combinedRoot);
    }

    /// <inheritdoc />
    public abstract Task UpdateAsync(CancellationToken cancellationToken = default);

    private IEnumerable<PhysicalSourceFile> IterateDirectory(Regex pattern, string path, string root)
    {
        var dir = new DirectoryInfo(path);
        var fileInfoEnumerable = dir.EnumerateFiles("**", new EnumerationOptions
        {
            AttributesToSkip = FileAttributes.System, // don't skip hidden files such as `.order`
            IgnoreInaccessible = true,
            MatchCasing = MatchCasing.CaseInsensitive,
            MatchType = MatchType.Win32,
            RecurseSubdirectories = true,
        });

        return from file in fileInfoEnumerable
               where file.Exists && pattern.IsMatch(file.Name)
               select new PhysicalSourceFile(this, FSPath.GetRelativePath(root, file.FullName));
    }
}