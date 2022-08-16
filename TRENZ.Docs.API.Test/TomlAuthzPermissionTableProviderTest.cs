using TRENZ.Docs.API.Services;

namespace TRENZ.Docs.API.Test;

[TestClass]
[TestCategory("Services")]
public class TomlAuthzPermissionTableProviderTest
{
    [TestMethod]
    public async Task TestGetPermissionTablesAsync()
    {
        var sources = TestHelper.ProvideFiles("./Data");
        var logger = TestHelper.GetLogger<TomlAuthzPermissionTableProvider>();
        var provider = new TomlAuthzPermissionTableProvider(sources, logger);

        var tables = await provider.GetPermissionTablesAsync().ToListAsync();

        Assert.AreEqual(3, tables.Count);

        var table = tables[0];
        var expectedLocation = new[] { "Test" };

        Assert.IsTrue(expectedLocation.SequenceEqual(table.LocationParts));

        Assert.IsTrue(table.Groups.ContainsKey("default"));
        Assert.IsTrue(table.Groups.ContainsKey("users"));

        var expectedUsersPermissions = new[] { "read" };
        Assert.IsTrue(expectedUsersPermissions.SequenceEqual(table.Groups["users"]));
    }
}