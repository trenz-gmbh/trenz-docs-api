using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models.Sources;
using TRENZ.Docs.API.Services;
using TRENZ.Docs.API.Test.Models.Sources;
using TRENZ.Docs.API.Test.Services;

namespace TRENZ.Docs.API.Test;

public static class TestHelper
{
    public static ILogger<T> GetLogger<T>()
    {
        var serviceProvider = new ServiceCollection()
            .AddLogging(b => b.AddDebug())
            .BuildServiceProvider();

        var factory = serviceProvider.GetRequiredService<ILoggerFactory>();
        return factory.CreateLogger<T>();
    }

    public static IConfiguration GetConfiguration(IEnumerable<KeyValuePair<string, string>> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    public static ISourcesProvider ProvideFiles(IEnumerable<MemorySourceFile> files)
    {
        var source = new MemorySource("Test", "/test/", "", files);
        return new MemorySourcesProvider(new[] { source });
    }

    public static ISourcesProvider ProvideFiles(string path)
    {
        var source = new LocalSource("Test", path);
        return new MemorySourcesProvider(new[] { source });
    }
}