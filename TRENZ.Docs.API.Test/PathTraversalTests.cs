using System.Security;

namespace TRENZ.Docs.API.Test;

[TestClass]
[TestCategory("Services")]
public class PathTraversalTests
{
    private const string MacOsRoot = "/var/folders/ab/cdefg/T/TRENZ.Docs.API/trenz-docs-test";
    private const string DotRoot = "./Data";
    private const string WindowsRoot = @"D:\www\docs.my.example";

    [DataTestMethod]
    [DataRow(MacOsRoot, "public/", $"{MacOsRoot}/public/")]
    [DataRow(MacOsRoot, "./", $"{MacOsRoot}/./")]
    [DataRow(WindowsRoot, @"public\", @$"{WindowsRoot}\public\")]
    public void TestResolvesPathAs(string root, string path, string expectedFullRoot)
    {
        var pathTraversalService = TestHelper.GetPathTraversalService();

        expectedFullRoot = NormalizeDirectorySeparatorChar(expectedFullRoot);

        var traversedPath = pathTraversalService.Traverse(root, path);

        traversedPath = NormalizeDirectorySeparatorChar(traversedPath);

        Assert.AreEqual(expectedFullRoot, traversedPath);

        static string NormalizeDirectorySeparatorChar(string expectedFullRoot)
        {
            foreach (var item in new[] { @"\", "/" })
                expectedFullRoot = expectedFullRoot.Replace(item, Path.DirectorySeparatorChar.ToString());
            return expectedFullRoot;
        }
    }

    [DataTestMethod]
    [DataRow(DotRoot, "./")]
    public void TestDoesNotThrowForPath(string root, string path)
    {
        var pathTraversalService = TestHelper.GetPathTraversalService();

        Assert.IsNotNull(pathTraversalService.Traverse(root, path));
    }

    [DataTestMethod]
    [DataRow(MacOsRoot, "../")]
    [DataRow(MacOsRoot, "/")]
    public void TestThrowsForPath(string root, string path)
    {
        var pathTraversalService = TestHelper.GetPathTraversalService();

        Assert.ThrowsException<SecurityException>(() =>
        {
            pathTraversalService.Traverse(root, path);
        });
    }
}
