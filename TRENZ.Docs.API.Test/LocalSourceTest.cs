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

            return LocalSource.FromConfiguration(configuration);
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
        var source = LocalSource.FromConfiguration(configuration);
        Assert.AreEqual(SourceType.Local, source.Type);
        Assert.AreEqual(config["Name"], source.Name);
        Assert.AreEqual(config["Root"], source.Root);
        Assert.AreEqual(config.ContainsKey("Path") ? config["Path"] : "", source.Path);
    }

    [TestMethod]
    public void TestUpdateAsync()
    {
        var configuration = TestHelper.GetConfiguration(new Dictionary<string, string> { { "Type", "local" }, { "Name", "name" }, { "Root", "root" } });
        var source = LocalSource.FromConfiguration(configuration);
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
        var source = LocalSource.FromConfiguration(configuration);
        Assert.AreEqual(expected, source.ToString());
    }

    public static IEnumerable<object[]> FindFilesValuesProvider()
    {
        var source = new LocalSource("Test", Path.Combine(Environment.CurrentDirectory, "Data"));

        yield return new object[]
        {
            ".*\\.md$",
            source,
            new[] { "Image.md", "Test.md", "Nested" + Path.DirectorySeparatorChar + "Text.md" },
        };

        yield return new object[]
        {
            "\\.order",
            source,
            new[] { ".order", "Nested" + Path.DirectorySeparatorChar + ".order" },
        };
    }

    [DataTestMethod]
    [DynamicData(nameof(FindFilesValuesProvider), DynamicDataSourceType.Method)]
    public void TestFindFiles(string pattern, ISource source, IEnumerable<string> relativePaths)
    {
        var files = source.FindFiles(new(pattern)).Select(sf => sf.RelativePath);

        Assert.IsTrue(relativePaths.SequenceEqual(files));
    }
}