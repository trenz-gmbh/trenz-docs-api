using TRENZ.Docs.API.Models;
using TRENZ.Docs.API.Models.Index;
using TRENZ.Docs.API.Services;

namespace TRENZ.Docs.API.Test;

[TestClass]
[TestCategory("Services")]
public class NodeFlaggingServiceTest
{
    public static IEnumerable<object[]> UpdateHasContentFlagValuesProvider()
    {
        yield return new object[]
        {
            new Dictionary<string, NavNode> { { "root", new("root") } },
            new List<IndexFile> { new("uid", "root", "root content", "root") },
            new Dictionary<string, NavNode> { { "root", new("root", true) } },
        };

        yield return new object[]
        {
            new Dictionary<string, NavNode>
            {
                {
                    "root", new("root", false, new()
                    {
                        { "nested", new("root/nested") },
                    })
                },
            },
            new List<IndexFile>
            {
                new("uid", "root", "root content", "root"),
                new("uid2", "nested", "nested content", "root/nested"),
            },
            new Dictionary<string, NavNode>
            {
                {
                    "root", new("root", true, new()
                    {
                        { "nested", new("root/nested", true) },
                    })
                },
            },
        };
    }

    [DataTestMethod]
    [DynamicData(nameof(UpdateHasContentFlagValuesProvider), DynamicDataSourceType.Method)]
    public async Task TestUpdateHasContentFlag(Dictionary<string, NavNode> tree, List<IndexFile> indexFiles, Dictionary<string, NavNode> expectedTree)
    {
        var service = new NodeFlaggingService();

        await service.UpdateHasContentFlag(tree, indexFiles);

        Assert.IsTrue(
            tree.DeepSequenceEquals(
                expectedTree,
                n => n.Value.Children,
                new TreeNodeEqualityComparer()
            )
        );
    }
}