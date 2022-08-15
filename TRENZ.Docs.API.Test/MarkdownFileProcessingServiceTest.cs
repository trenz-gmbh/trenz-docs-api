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
                    "E36B91F69E929E7E4CE4DE4C6C8A1919",
                    "Test",
                    "# Test",
                    "Test"
                ),
                new IndexFile(
                    "AC3AEF213ACC355D71D9E3A708283052",
                    "Image",
                    "![Image](%API_HOST%/File/test.png)\n![Image](%API_HOST%/File/../relative.png)\n![Image](%API_HOST%/File/images/public/nested.png)",
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
        var indexFiles = await service.ProcessAsync(sourceFiles.ToAsyncEnumerable()).ToListAsync();

        foreach (var (expected, output) in expectedFiles.Zip(indexFiles))
        {
            Assert.AreEqual(expected.uid, output.uid);
            Assert.AreEqual(expected.name, output.name);
            Assert.AreEqual(expected.content, output.content);
            Assert.AreEqual(expected.location, output.location);
        }
    }
}