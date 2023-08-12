using CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace AzSolutionManager.Core;

[Verb("destroy-all", HelpText = "Destroy ASM and all solutions.")]
public class DestroyAllOptions : BaseOptions
{
    protected override string GetOperationName()
    {
        return "Destroy ASM";
    }

    protected override void RunOperation(ServiceProvider serviceProvider)
    {
        var svc = serviceProvider.GetRequiredService<AzurePolicyGenerator>();
        svc.Destory();
    }
}
