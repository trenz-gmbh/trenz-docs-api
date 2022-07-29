using TRENZ.Docs.API.Models;
using TRENZ.Docs.API.Models.Index;
using TRENZ.Docs.API.Services;

namespace TRENZ.Docs.API.Test;

[TestClass]
[TestCategory("Services")]
public class TreeBuildingServiceTest
{
    public static IEnumerable<object[]> BuildTreeAsyncValuesProvider()
    {
        yield return new object[]
        {
            new List<IndexFile>
            {
                new("uid1", "node1", "node1 content", "node1"),
                new("uid2", "node2", "node2 content", "node2"),
                new("uid3", "node3", "node3 content", "node3"),
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
            new List<IndexFile>
            {
                new("uid1", "node1", "node1 content", "node1"),
                new("uid2", "node2", "node2 content", "nested/in/tree/node2"),
            },
            new Dictionary<string, NavNode>
            {
                { "node1", new("node1") },
                {
                    "nested", new("nested", false, new()
                    {
                        {
                            "in", new("nested/in", false, new()
                            {
                                {
                                    "tree", new("nested/in/tree", false, new()
                                    {
                                        { "node2", new("nested/in/tree/node2") },
                                    })
                                },
                            })
                        },
                    })
                },
            },
        };
    }

    [DataTestMethod]
    [DynamicData(nameof(BuildTreeAsyncValuesProvider), DynamicDataSourceType.Method)]
    public async Task TestBuildTreeAsync(IEnumerable<IndexFile> indexFiles, Dictionary<string, NavNode> expectedTree)
    {
        var service = new TreeBuildingService();
        var tree = await service.BuildTreeAsync(indexFiles);

        Assert.IsTrue(
            tree.DeepSequenceEquals(
                expectedTree,
                n => n.Value.Children,
                new TreeNodeEqualityComparer()
            )
        );
    }
}
