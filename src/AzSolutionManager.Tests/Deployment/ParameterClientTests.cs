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

		lookupClient.GetUniqueName(d.SolutionId, d.Enviroment, "foo", null, null).ReturnsNull();
		lookupClient.GetNameByResourceType(d.SolutionId, d.Enviroment, "Microsoft.Storage/storageAccounts", null, null).Returns("bar");

		parameterClient.CreateDeploymentParameters(environmentName: null, component: null);

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

		lookupClient.GetUniqueName(d.SolutionId, d.Enviroment, "foo", null, null).Returns("Soap");

		parameterClient.CreateDeploymentParameters(environmentName: null, component: null);

		lookupClient.DidNotReceive().GetNameByResourceType(d.SolutionId, d.Enviroment, "Microsoft.Storage/storageAccounts", null, component: null);
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

		lookupClient.GetUniqueName(d.SolutionId, "stage", "foo", null, null).Returns("Soap");

		parameterClient.CreateDeploymentParameters(environmentName: "stage", component: null);

		lookupClient.DidNotReceive().GetNameByResourceType(d.SolutionId, d.Enviroment, "Microsoft.Storage/storageAccounts", null, component: null);
		oneTimeOutWriter.Received().Write(Arg.Is<DeploymentOut>(x =>
		x.Parameters != null &&
		x.Parameters.ContainsKey("storageName") &&
		x.Parameters["storageName"].Value == "Soap"), false);
	}

	[TestMethod]
	public void WithExistingResourceTaggedAndComponentOverride_ReturnName()
	{
		var d = new ParameterDefination
		{
			SolutionId = "Solution1",
			Enviroment = "dev",
			Component = "database",
			Parameters = new Dictionary<string, string>()
			{
				{"storageName","@asm-resource-id:foo,@asm-resource-type:Microsoft.Storage/storageAccounts" }
			}
		};
		parameterDefinationLoader.Get().Returns(d);

		lookupClient.GetUniqueName(d.SolutionId, "dev", "foo", null, component: "api").Returns("Soap");

		parameterClient.CreateDeploymentParameters(environmentName: null, component: "api");

		lookupClient.DidNotReceive().GetNameByResourceType(d.SolutionId, d.Enviroment, "Microsoft.Storage/storageAccounts", null, component: null);
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

		lookupClient.GetUniqueName(d.SolutionId, d.Enviroment, "foo", null, null).ReturnsNull();
		lookupClient.GetNameByResourceType(d.SolutionId, d.Enviroment, "Microsoft.Storage/storageAccounts", null, component: null).ReturnsNull();

		parameterClient.CreateDeploymentParameters(environmentName: null, component: null);


		oneTimeOutWriter.Received().Write(Arg.Is<DeploymentOut>(x =>
		x.Parameters == null), false);
	}
}
