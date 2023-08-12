using Azure.ResourceManager.Authorization.Models;
using AzSolutionManager.Core;

namespace AzSolutionManager.Authorization;

public class RoleAssignmentClient
{
	private readonly IAzureClient azureClient;

	public RoleAssignmentClient(IAzureClient azureClient)
	{
		this.azureClient = azureClient;
	}

	public void Apply(string roleName, Guid principalId, string principalTypeStr, string solutionId, string environmentName, string? region)
	{
		var principalType = new RoleManagementPrincipalType(principalTypeStr);

		var group = (region is not null ?
			azureClient.GetResourceGroup(solutionId, environmentName, region) :
			azureClient.GetResourceGroup(solutionId, environmentName)) ?? throw new UserException("Group is not found. Please make sure you provide a valid solution Id, environment and optionally region.");
		var roleId = azureClient.GetRoleDefination(roleName) ?? throw new UserException($"Role {roleName} is not found. Please make sure you provide a valid role name.");
		azureClient.ApplyResourceGroupUserRole(group, roleId, principalId, principalType);
	}
}
