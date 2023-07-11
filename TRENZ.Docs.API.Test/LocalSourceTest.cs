using TRENZ.Docs.API.Models.Sources;

namespace TRENZ.Docs.API.Test;

[TestClass]
[TestCategory("Models")]
public class LocalSourceTest
{
    public static IEnumerable<object[]> InvalidConfigurationProvider()
    {
        yield return new object[] { new Dictionary<string, string>() };
        yield return new object[] { new Dictionary<string, string> { { "Type", "git" } } };
        yield return new object[] { new Dictionary<string, string> { { "Type", "local" } } };
        yield return new object[] { new Dictionary<string, string> { { "Type", "local" }, { "Name", "name" } } };
        yield return new object[] { new Dictionary<string, string> { { "Type", "local" }, { "Path", "path" } } };
        yield return new object[] { new Dictionary<string, string> { { "Type", "local" }, { "Root", "root" } } };
        yield return new object[] { new Dictionary<string, string> { { "Type", "git" }, { "Name", "name" }, { "Root", "root" } } };
    }

    [DataTestMethod]
    [DynamicData(nameof(InvalidConfigurationProvider), DynamicDataSourceType.Method)]
    public void TestConstructorThrowsForInvalidConfig(Dictionary<string, string> config)
    {
        Assert.ThrowsException<ArgumentException>(() =>
        {
            var configuration = TestHelper.GetConfiguration(config);
            var pathTraversalService = TestHelper.GetPathTraversalService();

            return LocalSource.FromConfiguration(configuration, pathTraversalService);
        });
    }

    public static IEnumerable<object[]> ValidConfigurationProvider()
    {
        yield return new object[] { new Dictionary<string, string> { { "Type", "local" }, { "Name", "name" }, { "Root", "root" } } };
        yield return new object[] { new Dictionary<string, string> { { "Type", "LOCAL" }, { "Name", "name" }, { "Root", "root" }, { "Path", "path" } } };
        yield return new object[] { new Dictionary<string, string> { { "Type", "LocAL" }, { "Name", "name" }, { "Root", "root" } } };
    }

    [DataTestMethod]
    [DynamicData(nameof(ValidConfigurationProvider), DynamicDataSourceType.Method)]
    public void TestConstructorDoesNotThrowForValidConfig(Dictionary<string, string> config)
    {
        var configuration = TestHelper.GetConfiguration(config);
        var pathTraversalService = TestHelper.GetPathTraversalService();

        var source = LocalSource.FromConfiguration(configuration, pathTraversalService);
        Assert.AreEqual(SourceType.Local, source.Type);
        Assert.AreEqual(config["Name"], source.Name);
        Assert.AreEqual(config["Root"], source.Root);
        Assert.AreEqual(config.ContainsKey("Path") ? config["Path"] : "", source.Path);
    }

    [TestMethod]
    public void TestUpdateAsync()
    {
        var configuration = TestHelper.GetConfiguration(new Dictionary<string, string> { { "Type", "local" }, { "Name", "name" }, { "Root", "root" } });
        var pathTraversalService = TestHelper.GetPathTraversalService();

        var source = LocalSource.FromConfiguration(configuration, pathTraversalService);
        var task = source.UpdateAsync();
        Assert.IsTrue(task.IsCompleted);
    }

    public static IEnumerable<object[]> ToStringValuesProvider()
    {
        yield return new object[] { new Dictionary<string, string> { { "Type", "local" }, { "Name", "name" }, { "Root", "root" } }, "Local Source: {Name: name, Root: root, Path: }" };
    }

    [DataTestMethod]
    [DynamicData(nameof(ToStringValuesProvider), DynamicDataSourceType.Method)]
    public void TestToString(Dictionary<string, string> config, string expected)
    {
        var configuration = TestHelper.GetConfiguration(config);
        var pathTraversalService = TestHelper.GetPathTraversalService();

        var source = LocalSource.FromConfiguration(configuration, pathTraversalService);
        Assert.AreEqual(expected, source.ToString());
    }

    public static IEnumerable<object[]> FindFilesValuesProvider()
    {
        var pathTraversalService = TestHelper.GetPathTraversalService();

        var source = new LocalSource(pathTraversalService, "Test", "./Data");

        yield return new object[]
        {
            ".*\\.md$",
            source,
            new[] { "Image.md", "Test.md", "Line-Breaks.md", Path.Combine("Nested", "Text.md") },
        };

        yield return new object[]
        {
            "\\.order",
            source,
            new[] { ".order", Path.Combine("Nested", ".order") },
        };
    }

    [DataTestMethod]
    [DynamicData(nameof(FindFilesValuesProvider), DynamicDataSourceType.Method)]
    public void TestFindFiles(string pattern, ISource source, IEnumerable<string> relativePaths)
    {
        var files = source.FindFiles(new(pattern)).Select(sf => sf.RelativePath);

        CollectionAssert.AreEquivalent(relativePaths.ToArray(), files.ToArray());
    }
}