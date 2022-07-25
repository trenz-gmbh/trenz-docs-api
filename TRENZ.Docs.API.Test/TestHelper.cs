using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
}