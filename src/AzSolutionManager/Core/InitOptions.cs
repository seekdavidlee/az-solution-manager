using CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace AzSolutionManager.Core;

[Verb("init", HelpText = "Setup ASM infrastructure on Azure Subscription.")]
public class InitOptions : BaseOptions
{
    [Option('g', "resource-group-name", HelpText = "Resource group name.", Required = true)]
    public string? ResourceGroupName { get; set; }

    [Option('l', "location", HelpText = "Resource group location.", Required = true)]
    public string? ResourceGroupLocation { get; set; }

    [Option('m', "managed-identity", HelpText = "Managed identity name.", Required = true)]
    public string? ManagedIdentityName { get; set; }

    protected override string GetOperationName()
    {
        return "Initialization";
    }

    protected override void RunOperation(ServiceProvider serviceProvider)
    {
        if (ResourceGroupName is null)
        {
            throw new Exception($"{nameof(ResourceGroupName)} cannot be null.");
        }

        if (ManagedIdentityName is null)
        {
            throw new Exception($"{nameof(ManagedIdentityName)} cannot be null.");
        }

        if (ResourceGroupLocation is null)
        {
            throw new Exception($"{nameof(ResourceGroupLocation)} cannot be null.");
        }

        var svc = serviceProvider.GetRequiredService<AzurePolicyGenerator>();

        svc.ApplyManagedIdentity(
            resourceGroupName: ResourceGroupName,
            managedIdentityName: ManagedIdentityName,
            resourceGroupLocation: ResourceGroupLocation);
    }
}
