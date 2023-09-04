using AzSolutionManager.Core;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace AzSolutionManager.Deployment;

[Verb("deployment", HelpText = "Helper function(s) to ARM/Bicep deployments in managed solutions.")]
public class DeploymentOptions : BaseOptions
{
	[Value(0, HelpText = "Valid option(s): parameters, run")]
	public string? Value { get; set; }

	[Option('f', "filepath", HelpText = "File path to deployment parameters file")]
	public string? FilePath { get; set; }

	[Option("template-filepath", HelpText = "Template file path.")]
	public string? TemplateFilePath { get; set; }

	[Option("deployment-name", HelpText = "Deployment name. If this is not provided, a timestamp based name will be provided.")]
	public string? DeploymentName { get; set; }

	[Option("complete-mode")]
	public bool CompleteMode { get; set; }

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

		if (verb == "run")
		{
			if (TemplateFilePath is null)
			{
				throw new UserException("Missing --template-filepath");
			}

			var svc = serviceProvider.GetRequiredService<ParameterClient>();

			string deploymentName = DeploymentName ?? DateTime.UtcNow.ToString("yyyyMMddhhmmss");
			svc.CreateAndRunDeployment(!CompleteMode, TemplateFilePath, deploymentName, ASMEnvironment, ASMComponent);
			return;
		}

		throw new UserException($"Option '{verb}' is invalid.");
	}
}
