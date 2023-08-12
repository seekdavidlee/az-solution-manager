using CommandLine.Text;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace AzSolutionManager.Core;

[Verb("destroy", HelpText = "Destroy Solution.")]
public class DestroyOptions : BaseOptions
{
    protected override string GetOperationName()
    {
        return "Destroy Solution";
    }

    protected override void RunOperation(ServiceProvider serviceProvider)
    {
        if (ASMSolutionId is null)
        {
            throw new UserException("Missing --asm-sol");
        }

        if (ASMEnvironment is null)
        {
            throw new UserException("Missing --asm-env");
        }

        var svc = serviceProvider.GetRequiredService<AzurePolicyGenerator>();
        svc.Destory(ASMSolutionId, ASMEnvironment);
    }
}
