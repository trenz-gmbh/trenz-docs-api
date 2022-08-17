using Moq;
using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models;
using TRENZ.Docs.API.Models.Sources;
using TRENZ.Docs.API.Services;
using TRENZ.Docs.API.Test.Models.Sources;

namespace TRENZ.Docs.API.Test;

[TestClass]
[TestCategory("Services")]
public class NavTreeProviderTest
{
    public static IEnumerable<object[]> BuildTreeAsyncValuesProvider()
    {
        yield return new object[]
        {
            new List<ISourceFile>
            {
                new MemorySourceFile("node1.md", "node1 content"),
                new MemorySourceFile("node2.md", "node2 content"),
                new MemorySourceFile("node3.md", "node3 content"),
                new MemorySourceFile("spaced-out.md", "name contains spaces content"),
                new MemorySourceFile("hy%2Dphened.md", "name contains hyphens content"),
            },
            new Dictionary<string, NavNode>
            {
                { "node1", new("node1") },
                { "node2", new("node2") },
                { "node3", new("node3") },
                { "spaced out", new("spaced out") },
                { "hy-phened", new("hy-phened") },
            },
        };

        yield return new object[]
        {
            new List<ISourceFile>
            {
                new MemorySourceFile("node1.md", "node1 content"),
                new MemorySourceFile(Path.Combine("nested", "in", "tree", "node2.md"), "node2 content"),
            },
            new Dictionary<string, NavNode>
            {
                { "node1", new("node1") },
                {
                    "nested", new("nested")
                    {
                        Children = new()
                        {
                            {
                                "in", new("nested/in")
                                {
                                    Children = new()
                                    {
                                        {
                                            "tree", new("nested/in/tree")
                                            {
                                                Children = new()
                                                {
                                                    { "node2", new("nested/in/tree/node2") },
                                                },
                                            }
                                        },
                                    },
                                }
                            },
                        },
                    }
                },
            },
        };
    }

    [DataTestMethod]
    [DynamicData(nameof(BuildTreeAsyncValuesProvider), DynamicDataSourceType.Method)]
    public async Task TestBuildTreeAsync(List<ISourceFile> indexFiles, Dictionary<string, NavNode> expectedTree)
    {
        var nodeFlaggerMock = new Mock<INavNodeFlaggingService>();
        nodeFlaggerMock.Setup(flagger => flagger.UpdateHasContentFlagAsync(new(), indexFiles, default)).Returns(Task.CompletedTask);
        var nodeOrderingMock = new Mock<INavNodeOrderingService>();
        nodeOrderingMock.Setup(orderer => orderer.ReorderTreeAsync(new(new(), false), default)).Returns(Task.CompletedTask);
        var nodeAuthorizationMock = new Mock<INavNodeAuthorizationService>();
        nodeAuthorizationMock.Setup(authorizer => authorizer.UpdateGroupsAsync(new(new(), false), default)).Returns(Task.CompletedTask);

        var service = new NavTreeProvider(nodeFlaggerMock.Object, nodeOrderingMock.Object, nodeAuthorizationMock.Object);
        var tree = await service.RebuildAsync(indexFiles);

        Assert.IsTrue(
            tree.Root.DeepSequenceEquals(
                expectedTree,
                n => n.Value.Children,
                new TreeNodeEqualityComparer()
            )
        );
    }
}