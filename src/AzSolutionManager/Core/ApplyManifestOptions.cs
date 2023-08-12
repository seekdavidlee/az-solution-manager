using CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace AzSolutionManager.Core;

[Verb("apply", HelpText = "Apply manifest definations to resource groups.")]
public class ApplyManifestOptions : BaseOptions
{
	[Option('f', "filepath", HelpText = "File path to manifest file")]
	public string? FilePath { get; set; }

	protected override string GetOperationName()
	{
		return "Apply manifest definations";
	}

	protected override void RunOperation(ServiceProvider serviceProvider)
	{
		var svc = serviceProvider.GetRequiredService<AzurePolicyGenerator>();
		var manifestLoader = serviceProvider.GetRequiredService<ManifestLoader>();
		svc.Apply(manifestLoader.Get());
	}
}
