using CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace AzSolutionManager.Core;

[Verb("destroy", HelpText = "Destroy ASM infrastructure and all solutions.")]
public class DestroyAllOptions : BaseOptions
{
	private const string operationName = "Destroy";

	protected override string GetOperationName()
    {
        return operationName;
    }

    protected override void RunOperation(ServiceProvider serviceProvider)
    {
        var svc = serviceProvider.GetRequiredService<AzurePolicyGenerator>();
        svc.Destory();
    }
}
