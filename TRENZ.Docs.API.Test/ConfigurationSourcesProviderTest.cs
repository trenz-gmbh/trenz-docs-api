using Microsoft.Extensions.Logging;
using TRENZ.Docs.API.Models.Sources;
using TRENZ.Docs.API.Services;

namespace TRENZ.Docs.API.Test;

[TestClass]
[TestCategory("Services")]
public class ConfigurationSourcesProviderTest
{
    [TestMethod]
    public void TestGetSources()
    {
        var config = TestHelper.GetConfiguration(new Dictionary<string, string>
        {
            { "Sources:0:Type", "local" },
            { "Sources:0:Name", "local test" },
            { "Sources:0:Root", "local root" },
            { "Sources:0:Path", "local path" },
            { "Sources:1:Type", "git" },
            { "Sources:1:Name", "git test" },
            { "Sources:1:Path", "git path" },
            { "Sources:1:Url", "https://localhost/git.git" },
            { "Sources:1:Branch", "main" },
        });

        var loggerFactory = new LoggerFactory();

        var provider = new ConfigurationSourcesProvider(config, loggerFactory);
        var sources = provider.GetSources().ToList();

        Assert.AreEqual(2, sources.Count);

        Assert.AreEqual(SourceType.Local, sources[0].Type);
        Assert.AreEqual("local test", sources[0].Name);
        Assert.AreEqual("local root", sources[0].Root);
        Assert.AreEqual("local path", sources[0].Path);

        Assert.AreEqual(SourceType.Git, sources[1].Type);
        Assert.AreEqual("git test", sources[1].Name);
        Assert.AreEqual("git path", sources[1].Path);
        Assert.IsFalse(string.IsNullOrEmpty(sources[1].Root));
    }

    [TestMethod]
    public void TestGetSources_InvalidType()
    {
        var config = TestHelper.GetConfiguration(new Dictionary<string, string>
        {
            { "Sources:0:Type", "invalid" },
            { "Sources:0:Name", "Test" },
        });

        var loggerFactory = new LoggerFactory();

        var provider = new ConfigurationSourcesProvider(config, loggerFactory);

        Assert.ThrowsException<NotImplementedException>(() => provider.GetSources().ToList(), "The source type 'invalid' is not implemented. Must be on of: 'git', 'local'");
    }
}