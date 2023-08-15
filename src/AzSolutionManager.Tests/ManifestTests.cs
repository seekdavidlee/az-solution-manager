using AzSolutionManager.Manifests;
using System.Reflection;
using System.Text.Json;

namespace AzSolutionManager.Tests;

[TestClass]
public class ManifestTests
{
    [TestMethod]
    public void ResourceGroupNamesDuplicated_GetException()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("AzSolutionManager.Tests.ManifestDupNames.json") ??
            throw new Exception("unable to get manifest stream for testing");

        var manifest = JsonSerializer.Deserialize<Manifest>(stream) ?? throw new Exception("unable to get manifest for testing");
        Assert.ThrowsException<Exception>(manifest.Validate);
    }
}
