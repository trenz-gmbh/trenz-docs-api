using TRENZ.Docs.API.Models;
using TRENZ.Docs.API.Models.Sources;
using TRENZ.Docs.API.Services;
using TRENZ.Docs.API.Test.Models.Sources;

namespace TRENZ.Docs.API.Test;

[TestClass]
[TestCategory("Services")]
public class NavNodeFlaggingServiceTest
{
    public static IEnumerable<object[]> UpdateHasContentFlagValuesProvider()
    {
        yield return new object[]
        {
            new Dictionary<string, NavNode> { { "root", new("root") } },
            new List<ISourceFile> { new MemorySourceFile("root.md", "root content") },
            new Dictionary<string, NavNode> { { "root", new("root") { HasContent = true } } },
        };

        yield return new object[]
        {
            new Dictionary<string, NavNode>
            {
                {
                    "root", new("root")
                    {
                        Children = new()
                        {
                            { "nested", new("root/nested") },
                        },
                    }
                },
            },
            new List<ISourceFile>
            {
                new MemorySourceFile("root.md", "root content"),
                new MemorySourceFile(Path.Combine("root", "nested.md"), "nested content"),
            },
            new Dictionary<string, NavNode>
            {
                {
                    "root", new("root")
                    {
                        HasContent = true,
                        Children = new()
                        {
                            {
                                "nested", new("root/nested")
                                {
                                    HasContent = true,
                                }
                            },
                        },
                    }
                },
            },
        };

        yield return new object[]
        {
            new Dictionary<string, NavNode>
            {
                {
                    "hyphen-no-content",
                    new("hyphen-no-content")
                },
                {
                    "hyphen-with-content",
                    new("hyphen-with-content")
                },
            },
            new List<ISourceFile>
            {
                new MemorySourceFile("hyphen%2Dwith%2Dcontent.md", "I am content"),
            },
            new Dictionary<string, NavNode>
            {
                {
                    "hyphen-no-content",
                    new("hyphen-no-content")
                },
                {
                    "hyphen-with-content",
                    new("hyphen-with-content")
                    {
                        HasContent = true,
                    }
                },
            },
        };

        yield return new object[]
        {
            new Dictionary<string, NavNode>
            {
                {
                    "space no content",
                    new("space no content")
                },
                {
                    "space with content",
                    new("space with content")
                },
            },
            new List<ISourceFile>
            {
                new MemorySourceFile("space-with-content.md", "I am content"),
            },
            new Dictionary<string, NavNode>
            {
                {
                    "space no content",
                    new("space no content")
                },
                {
                    "space with content",
                    new("space with content")
                    {
                        HasContent = true,
                    }
                },
            },
        };
    }

    [DataTestMethod]
    [DynamicData(nameof(UpdateHasContentFlagValuesProvider), DynamicDataSourceType.Method)]
    public async Task TestUpdateHasContentFlag(Dictionary<string, NavNode> tree, List<ISourceFile> files,
        Dictionary<string, NavNode> expectedTree)
    {
        var service = new NavNodeFlaggingService();

        await service.UpdateHasContentFlagAsync(tree, files);

        Assert.IsTrue(
            tree.DeepSequenceEquals(
                expectedTree,
                n => n.Value.Children,
                new TreeNodeEqualityComparer()
            )
        );
    }
}