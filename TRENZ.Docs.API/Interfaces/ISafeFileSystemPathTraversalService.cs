namespace TRENZ.Docs.API.Interfaces;

public interface ISafeFileSystemPathTraversalService
{
    string SafeCombine(string root, string path);
}
