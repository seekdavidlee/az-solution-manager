using AzSolutionManager.Core;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace AzSolutionManager.Authorization;

[Verb("role", HelpText = "Manage role assignments in managed solutions.")]
public class RoleOptions : BaseOptions
{
	[Value(0, HelpText = "Valid option(s): assign")]
	public string? Value { get; set; }

	[Option("role-name", HelpText = "Role name to apply to resource group.")]
	public string? RoleName { get; set; }

	[Option("principal-id", HelpText = "User or Group Id to apply to resource group.")]
	public Guid? PrincipalId { get; set; }

	[Option("principal-type", HelpText = "User, Group or ServicePrincipal.")]
	public string? PrincipalType { get; set; }

	private const string operationName = "Role";

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

		if (verb == "assign")
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
			return;
		}

		throw new UserException($"Option '{verb}' is invalid.");
	}
}
