using AzSolutionManager.Core;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace AzSolutionManager.Deployment;

[Verb("deployment", HelpText = "Helper function(s) to ARM/Bicep deployments in managed solutions.")]
public class DeploymentParametersOptions : BaseOptions
{
	[Value(0, HelpText = "Valid option(s): parameters")]
	public string? Value { get; set; }

	[Option('f', "filepath", HelpText = "File path to deployment parameters file")]
	public string? FilePath { get; set; }

	private const string operationName = "Deployment";

	protected override string GetOperationName()
	{
		return operationName;
	}

	protected override void RunOperation(ServiceProvider serviceProvider)
	{
		var verb = Value ?? throw new UserException("Missing option input.");
		if (verb == "parameters")
		{
			var svc = serviceProvider.GetRequiredService<ParameterClient>();
			svc.CreateDeploymentParameters(ASMEnvironment, ASMComponent);
			return;
		}

		throw new UserException($"Option '{verb}' is invalid.");
	}
}
