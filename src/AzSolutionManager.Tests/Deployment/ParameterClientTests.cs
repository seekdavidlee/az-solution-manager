using AzSolutionManager.Core;
using AzSolutionManager.Deployment;
using AzSolutionManager.Lookup;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace AzSolutionManager.Tests.Deployment;

[TestClass]
public class ParameterClientTests
{
	private readonly ILookupClient lookupClient;
	private readonly IOneTimeOutWriter oneTimeOutWriter;
	private readonly IParameterDefinationLoader parameterDefinationLoader;
	private readonly ParameterClient parameterClient;

	public ParameterClientTests()
	{
		lookupClient = Substitute.For<ILookupClient>();
		oneTimeOutWriter = Substitute.For<IOneTimeOutWriter>();
		parameterDefinationLoader = Substitute.For<IParameterDefinationLoader>();
		parameterClient = new ParameterClient(lookupClient, oneTimeOutWriter, parameterDefinationLoader);
	}

	[TestMethod]
	public void WithExistingResourceNotTagged_ReturnName()
	{
		var d = new ParameterDefination
		{
			SolutionId = "Solution1",
			Enviroment = "dev",
			Parameters = new Dictionary<string, string>()
			{
				{"storageName","@asm-resource-id:foo,@asm-resource-type:Microsoft.Storage/storageAccounts" }
			}
		};
		parameterDefinationLoader.Get().Returns(d);

		lookupClient.GetUniqueName(d.SolutionId, d.Enviroment, "foo", null).ReturnsNull();
		lookupClient.GetNameByResourceType(d.SolutionId, d.Enviroment, "Microsoft.Storage/storageAccounts", null).Returns("bar");

		parameterClient.CreateDeploymentParameters(environmentName: null);

		oneTimeOutWriter.Received().Write(Arg.Is<DeploymentOut>(x =>
		x.Parameters != null &&
		x.Parameters.ContainsKey("storageName") &&
		x.Parameters["storageName"].Value == "bar"), false);
	}

	[TestMethod]
	public void WithExistingResourceTagged_ReturnName()
	{
		var d = new ParameterDefination
		{
			SolutionId = "Solution1",
			Enviroment = "dev",
			Parameters = new Dictionary<string, string>()
			{
				{"storageName","@asm-resource-id:foo,@asm-resource-type:Microsoft.Storage/storageAccounts" }
			}
		};
		parameterDefinationLoader.Get().Returns(d);

		lookupClient.GetUniqueName(d.SolutionId, d.Enviroment, "foo", null).Returns("Soap");

		parameterClient.CreateDeploymentParameters(environmentName: null);

		lookupClient.DidNotReceive().GetNameByResourceType(d.SolutionId, d.Enviroment, "Microsoft.Storage/storageAccounts", null);
		oneTimeOutWriter.Received().Write(Arg.Is<DeploymentOut>(x =>
		x.Parameters != null &&
		x.Parameters.ContainsKey("storageName") &&
		x.Parameters["storageName"].Value == "Soap"), false);
	}

	[TestMethod]
	public void WithExistingResourceTaggedAndEnvironmentOverride_ReturnName()
	{
		var d = new ParameterDefination
		{
			SolutionId = "Solution1",
			Enviroment = "dev",
			Parameters = new Dictionary<string, string>()
			{
				{"storageName","@asm-resource-id:foo,@asm-resource-type:Microsoft.Storage/storageAccounts" }
			}
		};
		parameterDefinationLoader.Get().Returns(d);

		lookupClient.GetUniqueName(d.SolutionId, "stage", "foo", null).Returns("Soap");

		parameterClient.CreateDeploymentParameters(environmentName: "stage");

		lookupClient.DidNotReceive().GetNameByResourceType(d.SolutionId, d.Enviroment, "Microsoft.Storage/storageAccounts", null);
		oneTimeOutWriter.Received().Write(Arg.Is<DeploymentOut>(x =>
		x.Parameters != null &&
		x.Parameters.ContainsKey("storageName") &&
		x.Parameters["storageName"].Value == "Soap"), false);
	}

	[TestMethod]
	public void WithNoExistingResource_ParametersIsNotSet()
	{
		var d = new ParameterDefination
		{
			SolutionId = "Solution1",
			Enviroment = "dev",
			Parameters = new Dictionary<string, string>()
			{
				{"storageName","@asm-resource-id:foo,@asm-resource-type:Microsoft.Storage/storageAccounts" }
			}
		};
		parameterDefinationLoader.Get().Returns(d);

		lookupClient.GetUniqueName(d.SolutionId, d.Enviroment, "foo", null).ReturnsNull();
		lookupClient.GetNameByResourceType(d.SolutionId, d.Enviroment, "Microsoft.Storage/storageAccounts", null).ReturnsNull();

		parameterClient.CreateDeploymentParameters(environmentName: null);


		oneTimeOutWriter.Received().Write(Arg.Is<DeploymentOut>(x =>
		x.Parameters == null), false);
	}
}
