using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TRENZ.Docs.API.Interfaces;
using TRENZ.Docs.API.Models;
using TRENZ.Docs.API.Models.Sources;
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

    public static void AssertDeepSequenceEquals<T>(this IEnumerable<T>? a, IEnumerable<T>? b, Func<T, IEnumerable<T>?> childSelector, Action<T, T>? assertion)
    {
        assertion ??= Assert.AreEqual;

        if (a is null && b is null)
            return;

        Assert.IsNotNull(a);
        Assert.IsNotNull(b);

        var aList = a.ToList();
        var bList = b.ToList();

        Assert.AreEqual(aList.Count, bList.Count);

        for (var i = 0; i < aList.Count; i++)
        {
            var aItem = aList[i];
            var bItem = bList[i];

            assertion(aItem, bItem);

            childSelector(aItem).AssertDeepSequenceEquals(childSelector(bItem), childSelector, assertion);
        }
    }

    public static void AssertNavNodesAreEqual(KeyValuePair<string, NavNode> x, KeyValuePair<string, NavNode> y)
    {
        Assert.AreEqual(x.Key, y.Key);
        Assert.AreEqual(x.Value.Location, y.Value.Location);
        Assert.AreEqual(x.Value.Order, y.Value.Order);
        Assert.AreEqual(x.Value.ContainsUnauthorizedChildren, y.Value.ContainsUnauthorizedChildren);
        Assert.AreEqual(x.Value.HasContent, y.Value.HasContent);
        CollectionAssert.AreEqual(x.Value.Groups, y.Value.Groups);
    }

    public static void ForEachDeep<T>(this IEnumerable<T>? a, IEnumerable<T>? b, Action<T, T> action, Func<T, IEnumerable<T>?> childSelector)
    {
        if (a is null && b is null)
            return;

        if (a is null)
            throw new ArgumentNullException(nameof(a));

        if (b is null)
            throw new ArgumentNullException(nameof(b));

        var aList = a.ToList();
        var bList = b.ToList();

        if (aList.Count != bList.Count)
            throw new ArgumentException("Lists are not of equal length");

        for (var i = 0; i < aList.Count; i++)
        {
            var aItem = aList[i];
            var bItem = bList[i];

            action(aItem, bItem);

            childSelector(aItem).ForEachDeep(childSelector(bItem), action, childSelector);
        }
    }
}