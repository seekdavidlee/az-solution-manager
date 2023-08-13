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
			throw new UserException("Missing --asm-sol");
		}

		if (ASMEnvironment is null)
		{
			throw new UserException("Missing --asm-env");
		}

		if (RoleName is null)
		{
			throw new UserException("Missing --role-name");
		}

		if (PrincipalId is null)
		{
			throw new UserException("Missing --principal-id");
		}

		if (PrincipalType is null)
		{
			throw new UserException("Missing --principal-type");
		}

		var svc = serviceProvider.GetRequiredService<RoleAssignmentClient>();
		svc.Apply(RoleName, PrincipalId.Value, PrincipalType, ASMSolutionId, ASMEnvironment, ASMRegion);
	}
}
