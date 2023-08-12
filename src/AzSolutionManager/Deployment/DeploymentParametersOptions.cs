using AzSolutionManager.Core;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace AzSolutionManager.Deployment;

[Verb("deployment-parameters", HelpText = "Generate parameters used in bicep/ARM deployment. This is used with filepath parameter.")]
public class DeploymentParametersOptions : BaseOptions
{
    [Option('f', "filepath", HelpText = "File path to deployment parameters file")]
    public string? FilePath { get; set; }

    protected override string GetOperationName()
    {
        return "Generate deployment parameters";
    }

    protected override void RunOperation(ServiceProvider serviceProvider)
    {
        var svc = serviceProvider.GetRequiredService<ParameterClient>();
        svc.CreateDeploymentParameters();
    }
}
