﻿using TRENZ.Docs.API.Models.Index;
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
        yield return new object[] { "./test.md", false, "test.md", "test" };
        yield return new object[] { "assets/image.png", true, Path.Combine("nested", "document.md"), "%API_HOST%/File/nested/assets/image.png" };
        yield return new object[] { "../another_image.png", true, Path.Combine("nested", "document.md"), "%API_HOST%/File/another_image.png" };
        yield return new object[] { "path-with-hyphens/File%252DName.md", false, "links.md", "path%20with%20hyphens/File-Name" };
    }

    [DataTestMethod]
    [DynamicData(nameof(RewriteLinksValuesProvider), DynamicDataSourceType.Method)]
    public void TestRewriteLinks(string? original, bool isImage, string relativePath, string? expected)
    {
        var rewritten = MarkdownFileProcessingService.RewriteLinks(original, isImage, relativePath);

        Assert.AreEqual(expected, rewritten);
    }
}