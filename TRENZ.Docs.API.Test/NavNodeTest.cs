using TRENZ.Docs.API.Models;

namespace TRENZ.Docs.API.Test;

[TestClass]
[TestCategory("Models")]
public class NavNodeTest
{
    public static IEnumerable<object[]> CloneDataProvider()
    {
        yield return new object[] { new NavNode("location") };
        yield return new object[] { new NavNode("location") { Order = 1 } };
        yield return new object[] { new NavNode("location") { Order = 1, HasContent = true } };
        yield return new object[] { new NavNode("location") { Order = 1, HasContent = true, ContainsUnauthorizedChildren = true } };
        yield return new object[] { new NavNode("location") { Order = 1, HasContent = true, ContainsUnauthorizedChildren = true, Children = new() { { "child", new("location/child") } } } };
    }

    [DataTestMethod]
    [DynamicData(nameof(CloneDataProvider), DynamicDataSourceType.Method)]
    public void TestClone(NavNode original)
    {
        var clone = original.Clone();

        void CompareNodes(NavNode originalNode, NavNode clonedNode)
        {
            Assert.AreNotSame(originalNode, clonedNode);

            Assert.AreEqual(originalNode.Location, clonedNode.Location);
            Assert.AreEqual(originalNode.Order, clonedNode.Order);
            Assert.AreEqual(originalNode.HasContent, clonedNode.HasContent);
            Assert.AreEqual(originalNode.ContainsUnauthorizedChildren, clonedNode.ContainsUnauthorizedChildren);

            Assert.AreNotSame(original.Groups, clone.Groups);
            Assert.IsTrue(
                original.Groups
                    .Zip(clone.Groups)
                    .All(group =>
                    {
                        Assert.AreEqual(group.First.Key, group.Second.Key);
                        Assert.AreNotSame(group.First.Value, group.Second.Value);

                        return group.First.Value.Zip(group.Second.Value).All(pair => pair.First == pair.Second);
                    })
            );
        }

        CompareNodes(original, clone);

        if (original.Children == null)
            Assert.IsNull(clone.Children);
        else
        {
            Assert.AreNotSame(original.Children, clone.Children);
            original.Children.ForEachDeep(
                clone.Children,
                (a, b) =>
                {
                    Assert.AreEqual(a.Key, b.Key);
                    CompareNodes(a.Value, b.Value);
                },
                kvp => kvp.Value.Children
            );
        }
    }
}