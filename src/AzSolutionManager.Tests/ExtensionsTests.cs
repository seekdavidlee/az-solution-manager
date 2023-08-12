using Azure.Core;
using Azure.ResourceManager.Resources;
using AzSolutionManager.Core;

namespace AzSolutionManager.Tests;

[TestClass]
public class ExtensionsTests
{
    [TestMethod]
    public void ApplyTagsIfMissingOrTagValueIfDifferent()
    {
        var grp = new ResourceGroupData(AzureLocation.CentralUS);
        grp.Tags.Add("foo", "bar");

        var dic = new Dictionary<string, string>();
        dic["a1"] = "a1";
        dic["a2"] = "a2";

        Assert.IsTrue(grp.ApplyTagsIfMissingOrTagValueIfDifferent(dic));
        Assert.AreEqual("a1", grp.Tags["a1"]);
        Assert.AreEqual("a2", grp.Tags["a2"]);
        Assert.AreEqual("bar", grp.Tags["foo"]);
    }
}