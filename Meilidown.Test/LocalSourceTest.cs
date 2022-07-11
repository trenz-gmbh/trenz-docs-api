using Meilidown.Models.Sources;
using Microsoft.Extensions.Configuration;

namespace Meilidown.Test;

[TestClass]
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
        yield return new object[] { new Dictionary<string, string> { { "Type", "git" }, { "Name", "name" }, { "Path", "path" } } };
    }

    [DataTestMethod]
    [DynamicData(nameof(InvalidConfigurationProvider), DynamicDataSourceType.Method)]
    public void TestConstructorThrowsForInvalidConfig(Dictionary<string, string> config)
    {
        Assert.ThrowsException<ArgumentException>(() =>
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(config)
                .Build();

            return new LocalSource(configuration);
        });
    }
    
    public static IEnumerable<object[]> ValidConfigurationProvider()
    {
        yield return new object[] { new Dictionary<string, string> { { "Type", "local" }, { "Name", "name" }, { "Path", "path" } } };
        yield return new object[] { new Dictionary<string, string> { { "Type", "LOCAL" }, { "Name", "name" }, { "Path", "path" }, { "Root", "root" } } };
        yield return new object[] { new Dictionary<string, string> { { "Type", "LocAL" }, { "Name", "name" }, { "Path", "path" } } };
    }
    
    [DataTestMethod]
    [DynamicData(nameof(ValidConfigurationProvider), DynamicDataSourceType.Method)]
    public void TestConstructorDoesNotThrowForValidConfig(Dictionary<string, string> config)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(config)
            .Build();

        var source = new LocalSource(configuration);
        Assert.AreEqual(SourceType.Local, source.Type);
        Assert.AreEqual(config["Name"], source.Name);
        Assert.AreEqual(config["Path"], source.Path);
        Assert.AreEqual(config.ContainsKey("Root") ? config["Root"] : config["Path"], source.Root);
    }

    [TestMethod]
    public void TestUpdateAsync()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { { "Type", "local" }, { "Name", "name" }, { "Path", "path" } })
            .Build();
        
        var source = new LocalSource(configuration);
        var task = source.UpdateAsync();
        Assert.IsTrue(task.IsCompleted);
    }

    public static IEnumerable<object[]> ToStringValuesProvider()
    {
        yield return new object[] { new Dictionary<string, string> { { "Type", "local" }, { "Name", "name" }, { "Path", "path" } }, "Local Source: {Name: name, Root: path, Path: path}" };
    }
    
    [DataTestMethod]
    [DynamicData(nameof(ToStringValuesProvider), DynamicDataSourceType.Method)]
    public void TestToString(Dictionary<string, string> config, string expected)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(config)
            .Build();

        var source = new LocalSource(configuration);
        Assert.AreEqual(expected, source.ToString());
    }
}