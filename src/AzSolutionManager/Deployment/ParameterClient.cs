using AzSolutionManager.Core;
using AzSolutionManager.Lookup;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Resources.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace AzSolutionManager.Deployment;

public class ParameterClient
{
	private readonly ILookupClient lookupClient;
	private readonly IOneTimeOutWriter oneTimeOutWriter;
	private readonly IParameterDefinationLoader parameterDefinationLoader;
	private readonly IAzureClient azureClient;
	private readonly ILogger<ParameterClient> logger;

	public ParameterClient(
		ILookupClient lookupClient,
		IOneTimeOutWriter oneTimeOutWriter,
		IParameterDefinationLoader parameterDefinationLoader,
		IAzureClient azureClient,
		ILogger<ParameterClient> logger)
	{
		this.lookupClient = lookupClient;
		this.oneTimeOutWriter = oneTimeOutWriter;
		this.parameterDefinationLoader = parameterDefinationLoader;
		this.azureClient = azureClient;
		this.logger = logger;
	}

	private string GetAzCLIPath()
	{
		var paths = Environment.GetEnvironmentVariable("PATH");
		if (paths is null)
		{
			throw new Exception("PATH is empty!");
		}

		string[] pathDirs = paths.Split(Path.PathSeparator);

		foreach (string dir in pathDirs)
		{
			string azCmdPath = Path.Combine(dir, "az.cmd");

			if (File.Exists(azCmdPath))
			{
				return azCmdPath;
			}
		}

		throw new Exception("Unable to locate azure cli.");
	}

	public void CreateAndRunDeployment(bool incremental, string templateFilePath, string deploymentName, string? environmentName, string? component)
	{
		(DeploymentOut deploymentOut, ParameterDefination d) = GetDeploymentOut(environmentName, component);

		if (d.SolutionId is null)
		{
			throw new UserException("Missing configuring solutionId in your file.");
		}

		if (d.Enviroment is null)
		{
			throw new UserException("Missing configuring environment in your file.");
		}

		var groups = azureClient.GetResourceGroups(d.SolutionId, d.Enviroment, d.Region, d.Component);
		var found = groups.SingleOrDefault();

		if (found == null)
		{
			logger.LogWarning("No valid group is found with [{solutionId}, {enviroment}]", d.SolutionId, d.Enviroment);
			return;
		}

		string mode = incremental ? "Incremental" : "Complete";
		string args = $"deployment group create --name {deploymentName} --resource-group {found.Data.Name} --mode {mode} --no-prompt true --template-file {templateFilePath}";

		if (deploymentOut.Parameters is not null && deploymentOut.Parameters.Count > 0)
		{
			JsonSerializerOptions jsonOptions = new();
			jsonOptions.WriteIndented = false;
			var s = JsonSerializer.Serialize(deploymentOut.Parameters, jsonOptions).Replace("\"", "\\\"");
			args += $" --parameters {s}";
		}

		var start = new ProcessStartInfo()
		{
			FileName = GetAzCLIPath(),
			Arguments = args,
			RedirectStandardError = true,
			CreateNoWindow = false
		};

		logger.LogInformation("Running deployment: {deploymentName} on {resourceGroup}", deploymentName, found.Data.Name);
		using var proc = Process.Start(start);

		if (proc is not null)
		{
			proc.WaitForExit();

			if (proc.ExitCode != 0)
			{
				var err = proc.StandardError.ReadToEnd();
				throw new Exception(err is not null ? err : $"Deployment is not successful. Exit code: {proc.ExitCode}");
			}
		}
	}

	public void CreateDeploymentParameters(string? environmentName, string? component)
	{
		(DeploymentOut deploymentOut, ParameterDefination d) = GetDeploymentOut(environmentName, component);
		oneTimeOutWriter.Write(deploymentOut, d.CompressJsonOutput);
	}

	private (DeploymentOut, ParameterDefination) GetDeploymentOut(string? environmentName, string? component)
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

		return (deploymentOut, d);
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
