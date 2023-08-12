using AzSolutionManager.Core;
using NSubstitute;

namespace AzSolutionManager.Tests;

[TestClass]
public class ManifestTokenLookupTests
{
    private ManifestTokenLookup manifestTokenLookup;
    public ManifestTokenLookupTests()
    {
        var options = Substitute.For<IBaseOptions>();
        options.ASMResourceId.Returns("res1");
        options.ASMSolutionId.Returns("soln1");
        options.ASMRegion.Returns("reg1");
        options.ASMEnvironment.Returns("dev");

        manifestTokenLookup = new ManifestTokenLookup(options);
    }

    [TestMethod]
    public void ValueIsNotReplaced()
    {
        Assert.AreEqual("notokenvalue", manifestTokenLookup.Replace("notokenvalue"));
    }

    [DataTestMethod]
    [DataRow("soln1", "SolutionId")]
    [DataRow("res1", "ResourceId")]
    [DataRow("reg1", "Region")]
    [DataRow("dev", "Environment")]
    public void EntireValueIsReplacedWithAsmValue(string expected, string value)
    {
        Assert.AreEqual(expected, manifestTokenLookup.Replace($"@(asm.{value})"));
    }

    [DataTestMethod]
    [DataRow("soln1", "SolutionId")]
    [DataRow("res1", "ResourceId")]
    [DataRow("reg1", "Region")]
    [DataRow("dev", "Environment")]
    public void PrefixValueIsReplacedWithAsmValue(string expected, string value)
    {
        Assert.AreEqual($"abc-{expected}", manifestTokenLookup.Replace($"abc-@(asm.{value})"));
    }

    [DataTestMethod]
    [DataRow("soln1", "SolutionId")]
    [DataRow("res1", "ResourceId")]
    [DataRow("reg1", "Region")]
    [DataRow("dev", "Environment")]
    public void SuffixValueIsReplacedWithAsmValue(string expected, string value)
    {
        Assert.AreEqual($"{expected}-abc", manifestTokenLookup.Replace($"@(asm.{value})-abc"));
    }

    [DataTestMethod]
    [DataRow("soln1", "SolutionId")]
    [DataRow("res1", "ResourceId")]
    [DataRow("reg1", "Region")]
    [DataRow("dev", "Environment")]
    public void MiddleValueIsReplacedWithAsmValue(string expected, string value)
    {
        Assert.AreEqual($"abc-{expected}-efg", manifestTokenLookup.Replace($"abc-@(asm.{value})-efg"));
    }

    [DataTestMethod]
    [DataRow("soln1", "SolutionId", "res1", "ResourceId")]
    [DataRow("res1", "ResourceId", "reg1", "Region")]
    [DataRow("reg1", "Region", "dev", "Environment")]
    [DataRow("dev", "Environment", "soln1", "SolutionId")]
    public void MultipleMiddleValueIsReplacedWithAsmValue(
        string expected1, string value1, string expected2, string value2)
    {
        Assert.AreEqual($"abc-{expected1}-efg-{expected2}", manifestTokenLookup.Replace($"abc-@(asm.{value1})-efg-@(asm.{value2})"));
    }

    [TestMethod]
    public void NoValueIsReplacedWithSolutionId()
    {
        Assert.AreEqual("abc-@(asm.XXX)-efg", manifestTokenLookup.Replace("abc-@(asm.XXX)-efg"));
    }
}
