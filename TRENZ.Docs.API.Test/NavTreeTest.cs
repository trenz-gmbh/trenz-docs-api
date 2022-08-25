using TRENZ.Docs.API.Models;

namespace TRENZ.Docs.API.Test;

[TestClass]
[TestCategory("Models")]
public class NavTreeTest
{
    [TestMethod]
    public void TestWithoutHiddenNodes_Simple()
    {
        var tree = new NavTree(new()
        {
            { "root", new("root") { Order = 0 } },
            { "hidden", new("root") { Order = -1 } },
            { "node-supreme", new("node-supreme") { Order = 2 } },
            { "another node", new("another node") { Order = 1 } },
        });

        var expectedTree = new NavTree(new()
        {
            { "root", new("root") { Order = 0 } },
            { "node-supreme", new("node-supreme") { Order = 2 } },
            { "another node", new("another node") { Order = 1 } },
        });

        var actualTree = tree.WithoutHiddenNodes();
        Assert.AreEqual(expectedTree.ContainsUnauthorizedChildren, actualTree.ContainsUnauthorizedChildren);

        expectedTree.Root.AssertDeepSequenceEquals(
            actualTree.Root,
            kvp => kvp.Value.Children,
            TestHelper.AssertNavNodesAreEqual
        );
    }

    [TestMethod]
    public void TestWithoutHiddenNodes_Nested()
    {
        var tree = new NavTree(new()
        {
            {
                "root", new("root")
                {
                    Order = 0, HasContent = true, Children = new()
                    {
                        { "nested", new("root/nested") { Order = 0, HasContent = true } },
                        { "nested-hidden", new("root/nested-hidden") { Order = -1 } },
                    },
                }
            },
            { "hidden", new("root") { Order = -1 } },
        });

        var expectedTree = new NavTree(new()
        {
            {
                "root", new("root")
                {
                    Order = 0, HasContent = true, Children = new()
                    {
                        { "nested", new("root/nested") { Order = 0, HasContent = true } },
                    },
                }
            },
        });

        var actualTree = tree.WithoutHiddenNodes();
        Assert.AreEqual(expectedTree.ContainsUnauthorizedChildren, actualTree.ContainsUnauthorizedChildren);

        expectedTree.Root.AssertDeepSequenceEquals(
            actualTree.Root,
            kvp => kvp.Value.Children,
            TestHelper.AssertNavNodesAreEqual
        );
    }
}