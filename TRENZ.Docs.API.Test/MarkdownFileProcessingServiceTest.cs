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
                    "![Image](%API_HOST%/File/test.png)\n![Image](%API_HOST%/File/..%2frelative.png)\n![Image](%API_HOST%/File/images/public/nested.png)",
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
}