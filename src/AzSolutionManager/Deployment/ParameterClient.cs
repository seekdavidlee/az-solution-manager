using AzSolutionManager.Core;
using AzSolutionManager.Lookup;

namespace AzSolutionManager.Deployment;

public class ParameterClient
{
	private readonly ILookupClient lookupClient;
	private readonly IOneTimeOutWriter oneTimeOutWriter;
	private readonly IParameterDefinationLoader parameterDefinationLoader;

	public ParameterClient(
		ILookupClient lookupClient,
		IOneTimeOutWriter oneTimeOutWriter,
		IParameterDefinationLoader parameterDefinationLoader)
	{
		this.lookupClient = lookupClient;
		this.oneTimeOutWriter = oneTimeOutWriter;
		this.parameterDefinationLoader = parameterDefinationLoader;
	}

	public void CreateDeploymentParameters(string? environmentName, string? component)
	{
		var d = parameterDefinationLoader.Get();

		if (d.Parameters is null)
		{
			throw new UserException("Parameters must have at least one defination.");
		}

		if (d.SolutionId is null)
		{
			throw new UserException("Missing configuring solutionId in your file.");
		}

		// Override if configured.
		if (environmentName is not null)
		{
			d.Enviroment = environmentName;
		}

		if (component is not null)
		{
			d.Component = component;
		}

		if (d.Enviroment is null)
		{
			throw new UserException("Missing configuring environment in your file.");
		}

		var deploymentOut = new DeploymentOut
		{
			GroupName = lookupClient.GetResourceGroupName(d.SolutionId, d.Enviroment, d.Component)
		};

		foreach (var p in d.Parameters)
		{
			Parse(p, d.SolutionId, d.Enviroment, d.Region, d.Component, deploymentOut);
		}

		oneTimeOutWriter.Write(deploymentOut, d.CompressJsonOutput);
	}

	private void Parse(KeyValuePair<string, string> p, string solutionId, string environment, string? region, string? component, DeploymentOut deploymentOut)
	{
		var conditions = p.Value.Split(',');
		foreach (var condition in conditions)
		{
			var h = condition.Split(':');

			string? value = null;

			if (h.Length == 1)
			{
				value = p.Value;
			}
			else if (h.Length == 2)
			{
				value = h[0] switch
				{
					"@env" => Environment.GetEnvironmentVariable(h[1]),
					"@asm-resource-id" => lookupClient.GetUniqueName(solutionId, environment, h[1], region, component: component),
					"@asm-resource-type" => lookupClient.GetNameByResourceType(solutionId, environment, h[1], region, component: component),
					_ => throw new Exception($"{h[0]} is not a valid.")
				};
			}

			if (value is not null)
			{
				deploymentOut.Add(p.Key, value);
				return;
			}
		}

		// Do not throw an exception if it is missing. This is expected if resource has not been created yet.
	}
}
