﻿using Moq;
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
                new MemorySourceFile("uid1", "node1", "node1", "node1 content"),
                new MemorySourceFile("uid2", "node2", "node2", "node2 content"),
                new MemorySourceFile("uid3", "node3", "node3", "node3 content"),
            },
            new Dictionary<string, NavNode>
            {
                { "node1", new("node1") },
                { "node2", new("node2") },
                { "node3", new("node3") },
            },
        };

        yield return new object[]
        {
            new List<ISourceFile>
            {
                new MemorySourceFile("uid1", "node1", "node1", "node1 content"),
                new MemorySourceFile("uid2", "node2", "nested/in/tree/node2", "node2 content"),
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