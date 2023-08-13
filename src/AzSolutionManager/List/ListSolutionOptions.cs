using AzSolutionManager.Core;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace AzSolutionManager.List;

[Verb("list", HelpText = "List all solutions managed in Subscription. You can pass in --asm-reg or --asm-env to filter the results further.")]
public class ListSolutionOptions : BaseOptions
{
	protected override string GetOperationName()
	{
		return "List";
	}

	protected override void RunOperation(ServiceProvider serviceProvider)
	{
		var svc = serviceProvider.GetRequiredService<ListSolutionClient>();
		svc.GetSolutions(ASMRegion, ASMEnvironment);
	}
}
