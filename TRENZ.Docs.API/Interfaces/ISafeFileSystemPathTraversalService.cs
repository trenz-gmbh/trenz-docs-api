namespace TRENZ.Docs.API.Interfaces;

public interface ISafeFileSystemPathTraversalService
{
    string Traverse(string root, string path);
}
