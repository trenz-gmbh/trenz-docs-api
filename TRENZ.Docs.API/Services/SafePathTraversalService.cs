using System.Security;

using TRENZ.Docs.API.Interfaces;

using FSPath = System.IO.Path;

namespace TRENZ.Docs.API.Services;

/// <summary>
/// Combines a root path with a given relative path, avoiding some naïve
/// attempts to escape the jail (such as passing `/` or `..` in the relative
/// path).
/// </summary>
public class SafeFileSystemPathTraversalService : ISafeFileSystemPathTraversalService
{
    private static SecurityException PreventedPathTraversal
        => new("Prevented an attempt to traverse a path outside the source's root.");

    public string Traverse(string root, string path)
    {
        var combinedRoot = FSPath.Combine(root, path);

        if (!FSPath.GetFullPath(combinedRoot).StartsWith(root))
            throw PreventedPathTraversal;

        return combinedRoot;
    }
}
