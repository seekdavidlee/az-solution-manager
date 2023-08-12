using AzSolutionManager.Core;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace AzSolutionManager.Authorization;

[Verb("role-assignment", HelpText = "Lookup resource or group.")]
public class RoleAssignmentOptions : BaseOptions
{
    [Option("role-name", HelpText = "Role name to apply to resource group.")]
    public string? RoleName { get; set; }

    [Option("principal-id", HelpText = "User or Group Id to apply to resource group.")]
    public Guid? PrincipalId { get; set; }

    [Option("principal-type", HelpText = "User, Group or ServicePrincipal.")]
    public string? PrincipalType { get; set; }

    protected override string GetOperationName()
    {
        return "Role assignment";
    }

    protected override void RunOperation(ServiceProvider serviceProvider)
    {
        if (ASMSolutionId is null)
        {
            throw new Exception("asmSolutionId cannot be null.");
        }

        if (ASMEnvironment is null)
        {
            throw new Exception("asmEnvironment cannot be null.");
        }

        if (RoleName is null)
        {
            throw new Exception("RoleName cannot be null.");
        }

        if (PrincipalId is null)
        {
            throw new Exception("PrincipalId cannot be null.");
        }

        if (PrincipalType is null)
        {
            throw new Exception("PrincipalType cannot be null.");
        }

        var svc = serviceProvider.GetRequiredService<RoleAssignmentClient>();
        svc.Apply(RoleName, PrincipalId.Value, PrincipalType, ASMSolutionId, ASMEnvironment, ASMRegion);
    }
}
