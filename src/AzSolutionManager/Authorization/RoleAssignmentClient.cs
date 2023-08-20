using Azure.ResourceManager.Authorization.Models;
using AzSolutionManager.Core;
using Microsoft.Extensions.Logging;

namespace AzSolutionManager.Authorization;

public class RoleAssignmentClient
{
	private readonly IAzureClient azureClient;
	private readonly ILogger<RoleAssignmentClient> logger;

	public RoleAssignmentClient(IAzureClient azureClient, ILogger<RoleAssignmentClient> logger)
	{
		this.azureClient = azureClient;
		this.logger = logger;
	}

	public void Apply(string roleName, Guid principalId, string principalTypeStr, string solutionId, string environmentName, string? region, string? component)
	{
		var principalType = new RoleManagementPrincipalType(principalTypeStr);
		var roleId = azureClient.GetRoleDefination(roleName) ?? throw new UserException($"Role {roleName} is not found. Please make sure you provide a valid role name.");

		var groups = azureClient.GetResourceGroups(solutionId, environmentName, region, component);
		foreach (var group in groups)
		{
			logger.LogInformation("Apply role {roleName} on {groupName} for {principalId}", roleName, group.Data.Name, principalId);
			azureClient.ApplyResourceGroupUserRole(group, roleId, principalId, principalType);
		}
	}
}
