using TRENZ.Docs.API.Models;
using TRENZ.Docs.API.Services;
using TRENZ.Docs.API.Test.Models.Sources;

namespace TRENZ.Docs.API.Test;

[TestClass]
[TestCategory("Services")]
public class NavNodeOrderingServiceTest
{
    [TestMethod]
    public async Task TestReorderSimpleTree()
    {
        var sources = TestHelper.ProvideFiles(new[]
        {
            new MemorySourceFile("uid1","End", "End", "I'm at the end"),
            new MemorySourceFile("uid2","Start", "Start", "I'm at the start"),
            new MemorySourceFile("uid3","Middle", "Middle", "I'm in the middle"),
            new MemorySourceFile("uid4","Hidden", "Hidden", "I'm hidden"),
        });
        var logger = TestHelper.GetLogger<NavNodeOrderingService>();
        var service = new NavNodeOrderingService(sources, logger);

        var tree = new NavTree(new()
        {
            { "End", new("End") },
            { "Start", new("Start") },
            { "Middle", new("Middle") },
            { "Hidden", new("Hidden") },
        });

        Assert.IsTrue(tree.Root.All(kvp => kvp.Value.Order == 0));

        await service.ReorderTreeAsync(tree);

        var actualOrder = tree.Root.Select(kvp => kvp.Value.Order);
        var expectedOrder = new[] { 0, 3, 2, 1 };

        Assert.IsTrue(actualOrder.SequenceEqual(expectedOrder));
    }

    [TestMethod]
    public async Task TestReorderSimpleTreeWithOrderFile()
    {
        var sources = TestHelper.ProvideFiles(new[]
        {
            new MemorySourceFile("uid1","End", "End", "I'm at the end"),
            new MemorySourceFile("uid2","Start", "Start", "I'm at the start"),
            new MemorySourceFile("uid3","Middle", "Middle", "I'm in the middle"),
            new MemorySourceFile("uid4","Hidden", "Hidden", "I'm hidden"),
            new MemorySourceFile("uid5",".order", ".order", "Start\r\nMiddle\r\nEnd"),
        });
        var logger = TestHelper.GetLogger<NavNodeOrderingService>();
        var service = new NavNodeOrderingService(sources, logger);

        var tree = new NavTree(new()
        {
            { "End", new("End") },
            { "Start", new("Start") },
            { "Middle", new("Middle") },
            { "Hidden", new("Hidden") },
        });

        Assert.IsTrue(tree.Root.All(kvp => kvp.Value.Order == 0));

        await service.ReorderTreeAsync(tree);

        var actualOrder = tree.Root.Select(kvp => kvp.Value.Order);
        var expectedOrder = new[] { 2, 0, 1, -1 };

        Assert.IsTrue(actualOrder.SequenceEqual(expectedOrder));
    }
}
