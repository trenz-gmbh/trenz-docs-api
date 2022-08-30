using TRENZ.Docs.API.Models.Index;
using TRENZ.Docs.API.Models.Sources;
using TRENZ.Docs.API.Services;

namespace TRENZ.Docs.API.Test;

[TestClass]
[TestCategory("Services")]
public class MarkdownFileProcessingServiceTest
{
    public static IEnumerable<object[]> ProcessFileDataProvider()
    {
        var testConfig = new LocalSource("Test", Path.Combine(Environment.CurrentDirectory, "Data"));

        yield return new object[]
        {
            new[]
            {
                new PhysicalSourceFile(testConfig, "Test.md"),
                new PhysicalSourceFile(testConfig, "Image.md"),
                new PhysicalSourceFile(testConfig, "Line-Breaks.md"),
            },
            new[]
            {
                new IndexFile(
                    "0CBC6611F5540BD0809A388DC95A615B",
                    "Test",
                    "# Test",
                    "Test"
                ),
                new IndexFile(
                    "BE53A0541A6D36F6ECB879FA2C584B08",
                    "Image",
                    "![Image](%API_HOST%/File/test.png)\n![Image](%API_HOST%/File/images/public/nested.png)",
                    "Image"
                ),
                new IndexFile(
                    "9A6FEDC1A875D0EF0E404F27A48B43E6",
                    "Line Breaks",
                    "A database will have one or more **tables**, each of which contains\n**rows**.",
                    "Line Breaks"
                ),
            },
        };
    }

    [DataTestMethod]
    [DynamicData(nameof(ProcessFileDataProvider), DynamicDataSourceType.Method)]
    public async Task TestProcessAsync(IEnumerable<PhysicalSourceFile> sourceFiles, IEnumerable<IndexFile> expectedFiles)
    {
        var logger = TestHelper.GetLogger<MarkdownFileProcessingService>();
        var service = new MarkdownFileProcessingService(logger);
        var indexFiles = await service.ProcessAsync(sourceFiles).ToListAsync();

        foreach (var (expected, output) in expectedFiles.Zip(indexFiles))
        {
            Assert.AreEqual(expected.uid, output.uid);
            Assert.AreEqual(expected.name, output.name);
            Assert.AreEqual(expected.content, output.content);
            Assert.AreEqual(expected.location, output.location);
        }
    }

    public static IEnumerable<object[]> RewriteLinksValuesProvider()
    {
        yield return new object[] { "./test.md", false, "test.md", "/wiki/test" };
        yield return new object[] { "assets/image.png", true, Path.Combine("nested", "document.md"), "%API_HOST%/File/nested/assets/image.png" };
        yield return new object[] { "../another_image.png", true, Path.Combine("nested", "document.md"), "%API_HOST%/File/another_image.png" };
        yield return new object[] { "../another_page.md", false, Path.Combine("nested", "document.md"), "/wiki/another_page" };
        yield return new object[] { "../another/nested-page.md", false, Path.Combine("nested", "document.md"), "/wiki/another/nested%20page" };
        // go one level up, then a different folder back down
        yield return new object[] { "../a2/a21.md", false, Path.Combine("a", "a1", "a11.md"), "/wiki/a/a2/a21" };
        yield return new object[] { "path-with-hyphens/File%252DName.md", false, "links.md", "/wiki/path%20with%20hyphens/File-Name" };
        yield return new object[] { "https://google.com/", false, "external.md", "https://google.com/" };
    }

    [DataTestMethod]
    [DynamicData(nameof(RewriteLinksValuesProvider), DynamicDataSourceType.Method)]
    public void TestRewriteLinks(string original, bool isImage, string relativePath, string? expected)
    {
        var rewritten = MarkdownFileProcessingService.RewriteLinks(original, isImage, relativePath);

        Assert.AreEqual(expected, rewritten);
    }

    [TestMethod]
    public void TestRewriteLinksThrowsForPathsOutsideSource()
    {
        Assert.ThrowsException<ArgumentException>(() => MarkdownFileProcessingService.RewriteLinks("../test.md", false, "other.md"));
        Assert.ThrowsException<ArgumentException>(() => MarkdownFileProcessingService.RewriteLinks("../../test.png", true, "nested/doc.md"));
    }
}