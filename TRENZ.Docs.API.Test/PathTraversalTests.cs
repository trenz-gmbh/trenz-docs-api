using System.Security;

namespace TRENZ.Docs.API.Test;

[TestClass]
[TestCategory("Services")]
public class PathTraversalTests
{
    private const string MacOsRoot = "/var/folders/ab/cdefg/T/TRENZ.Docs.API/trenz-docs-test";

    [DataTestMethod]
    [DataRow(MacOsRoot, "public/", $"{MacOsRoot}/public/")]
    [DataRow(MacOsRoot, "./", $"{MacOsRoot}/./")]
    public void TestAllowsPaths(string root, string path, string expectedFullRoot)
    {
        var pathTraversalService = TestHelper.GetPathTraversalService();

        Assert.AreEqual(expectedFullRoot, pathTraversalService.Traverse(root, path));
    }

    [DataTestMethod]
    [DataRow(MacOsRoot, "../")]
    [DataRow(MacOsRoot, "/")]
    public void TestThrowsForPaths(string root, string path)
    {
        var pathTraversalService = TestHelper.GetPathTraversalService();

        Assert.ThrowsException<SecurityException>(() =>
        {
            pathTraversalService.Traverse(root, path);
        });
    }
}
