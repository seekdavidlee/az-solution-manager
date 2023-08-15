using AzSolutionManager.Core;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace AzSolutionManager.Manifests;

[Verb("manifest", HelpText = "Manage manifest definations.")]
public class ManifestOptions : BaseOptions
{
	[Option('f', "filepath", HelpText = "File path to manifest file")]
	public string? FilePath { get; set; }

	[Value(0, HelpText = "Valid option(s): apply")]
	public string? Value { get; set; }

	private const string operationName = "Manifest";

	protected override string GetOperationName()
	{
		return operationName;
	}

	protected override void RunOperation(ServiceProvider serviceProvider)
	{
		var verb = this.Value;
		if (verb is null)
		{
			throw new UserException("Missing option input.");
		}

		if (verb == "apply")
		{
			var svc = serviceProvider.GetRequiredService<AzurePolicyGenerator>();
			var manifestLoader = serviceProvider.GetRequiredService<ManifestLoader>();
			svc.Apply(manifestLoader.Get());
			return;
		}

		throw new UserException($"Option '{verb}' is invalid.");
	}
}
