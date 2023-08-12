using Azure.ResourceManager.Resources;
using Azure.ResourceManager;
using Azure.Identity;
using Azure;
using Azure.Core;
using Microsoft.Extensions.Logging;
using Azure.ResourceManager.Resources.Models;
using Azure.ResourceManager.Authorization;
using Azure.ResourceManager.Authorization.Models;
using Azure.ResourceManager.Models;
using AzSolutionManager.Lookup;
using System.Security.AccessControl;

namespace AzSolutionManager.Core;

public class AzureClient : IAzureClient
{
	private readonly ArmClient client;
	private readonly string subscriptionId;
	private readonly SubscriptionResource subscriptionResource;
	private readonly ILogger<AzureClient> logger;
	private readonly IBaseOptions options;

	public AzureClient(ILogger<AzureClient> logger, IBaseOptions options)
	{
		if (options.Tenant is not null)
		{
			client = new ArmClient(new DefaultAzureCredential(new DefaultAzureCredentialOptions
			{
				TenantId = options.Tenant
			}));
		}
		else
		{
			client = new ArmClient(new DefaultAzureCredential());
		}

		SubscriptionResource? found = null;
		if (options.Subscription is not null)
		{
			found = Guid.TryParse(options.Subscription, out _) ?
				client.GetSubscriptionResource(new ResourceIdentifier(string.Format(Constants.SubscriptionsPrefix, options.Subscription))) :
				client.GetSubscriptions().FirstOrDefault(x => string.Equals(x.Data.DisplayName, options.Subscription, StringComparison.OrdinalIgnoreCase));
		}

		subscriptionResource = found ?? client.GetDefaultSubscription();

		if (subscriptionResource.Id.SubscriptionId is null)
		{
			throw new Exception("Unexpected for subscription Id to be null.");
		}

		subscriptionId = subscriptionResource.Id.SubscriptionId;
		this.logger = logger;
		this.options = options;
	}

	private readonly Dictionary<string, ResourceGroupResource> _resourceGroupsCache = new();

	public ResourceGroupResource? GetResourceGroup(string solutionId, string environment)
	{
		string key = $"{solutionId}.{environment}";
		if (_resourceGroupsCache.TryGetValue(key, out ResourceGroupResource? value))
		{
			return value;
		}

		var resourceGroups = subscriptionResource.GetResourceGroups();
		var groups = resourceGroups.GetAll($"tagName eq '{Constants.AsmSolutionId}' and tagValue eq '{solutionId}'");
		foreach (var g in groups)
		{
			if (g.Data.Tags[Constants.AsmEnvironment] == environment)
			{
				_resourceGroupsCache.Add(key, g);
				return g;
			}
		}

		return default;
	}

	public ResourceGroupResource? GetResourceGroup(string solutionId, string environment, string region)
	{
		string key = $"{solutionId}.{environment}.{region}";
		if (_resourceGroupsCache.TryGetValue(key, out ResourceGroupResource? value))
		{
			return value;
		}

		var resourceGroups = subscriptionResource.GetResourceGroups();
		var groups = resourceGroups.GetAll($"tagName eq '{Constants.AsmSolutionId}' and tagValue eq '{solutionId}'");
		foreach (var g in groups)
		{
			if (g.Data.Tags[Constants.AsmEnvironment] == environment && g.Data.Tags[Constants.AsmRegion] == region)
			{
				_resourceGroupsCache.Add(key, g);
				return g;
			}
		}

		return default;
	}

	public void DeleteAllResourceGroups()
	{
		var resourceGroups = subscriptionResource.GetResourceGroups();
		var groups = resourceGroups.GetAll($"tagName eq '{Constants.AsmInternalSolutionId}' and tagValue eq '{GetASMInternalSolutionIdValue()}'");
		foreach (var group in groups)
		{
			var locks = group.GetManagementLocks();
			bool preventDelete = false;
			foreach (var mlock in locks)
			{
				if (mlock.Data.Name.EndsWith(Constants.LockNameSuffix) &&
					mlock.Data.Notes == Constants.LockNotes)
				{
					mlock.Delete(WaitUntil.Completed);
					logger.LogInformation("Deleted ASM created lock '{lockName}'", mlock.Data.Name);
				}
				else
				{
					preventDelete = true;
				}
			}

			if (preventDelete)
			{
				logger.LogWarning("Unable to remove resource group {resourceGroupName} as there are locks. Please manually remove the lock(s) and try again.", group.Data.Name);
				continue;
			}

			group.Delete(WaitUntil.Completed);
			logger.LogInformation("Deleted resource group {resourceGroupName}", group.Data.Name);
		}
	}

	public void DeleteAllPolicies()
	{
		var subscriptionPolicyDefinitions = subscriptionResource.GetSubscriptionPolicyDefinitions();

		foreach (var policy in subscriptionPolicyDefinitions.Where(x => x.Data.DisplayName.StartsWith(Constants.PolicySpecificPrefix)))
		{
			var meta = policy.Data.Metadata.ToDictionary();
			if (meta is not null && meta.TryGetValue(Constants.AsmInternalSolutionId, out var asmInternalSolutionId))
			{
				if (asmInternalSolutionId == GetASMInternalSolutionIdValue())
				{
					try
					{
						policy.Delete(WaitUntil.Completed);
						logger.LogInformation("Deleted policy '{policyName}'", policy.Data.Name);
					}
					catch (RequestFailedException rEx)
					{
						logger.LogError(rEx, "Unable to remove policy '{policyId}'", policy.Data.Name);
					}
				}
			}
		}
	}

	private void DeleteAllPolicies(string solutionId, string environment)
	{
		var subscriptionPolicyDefinitions = subscriptionResource.GetSubscriptionPolicyDefinitions();

		foreach (var policy in subscriptionPolicyDefinitions.Where(x => x.Data.DisplayName.StartsWith(Constants.PolicySpecificPrefix)))
		{
			var meta = policy.Data.Metadata.ToDictionary();
			if (meta is not null &&
				meta.TryGetValue(Constants.AsmInternalSolutionId, out var asmInternalSolutionId) &&
				meta.TryGetValue(Constants.AsmSolutionId, out var asmSolutionId) &&
				meta.TryGetValue(Constants.AsmEnvironment, out var asmEnvironment))
			{
				if (asmInternalSolutionId == GetASMInternalSolutionIdValue() &&
					asmSolutionId == solutionId &&
					asmEnvironment == environment)
				{
					try
					{
						policy.Delete(WaitUntil.Completed);
						logger.LogInformation("Deleted policy '{policyName}'", policy.Data.Name);
					}
					catch (RequestFailedException rEx)
					{
						logger.LogError(rEx, "Unable to remove policy '{policyId}'", policy.Data.Name);
					}
				}
			}
		}
	}

	public void DeleteResourceGroupsAndPolicies(string solutionId, string environment)
	{
		var resourceGroups = subscriptionResource.GetResourceGroups();
		var groups = resourceGroups.GetAll($"tagName eq '{Constants.AsmSolutionId}' and tagValue eq '{solutionId}'")
			.Where(x => x.Data.Tags[Constants.AsmEnvironment] == environment);
		foreach (var group in groups)
		{
			var locks = group.GetManagementLocks();
			bool preventDelete = false;
			foreach (var mlock in locks)
			{
				if (mlock.Data.Name.EndsWith(Constants.LockNameSuffix) &&
					mlock.Data.Notes == Constants.LockNotes)
				{
					mlock.Delete(WaitUntil.Completed);
					logger.LogInformation("Deleted ASM created lock '{lockName}'", mlock.Data.Name);
				}
				else
				{
					preventDelete = true;
				}
			}

			if (preventDelete)
			{
				logger.LogWarning("Unable to remove resource group {resourceGroupName} as there are locks. Please manually remove the lock(s) and try again.", group.Data.Name);
				continue;
			}

			var policiesAssignments = group.GetPolicyAssignments();
			foreach (var policyAssignment in policiesAssignments)
			{
				if (!policyAssignment.Data.Name.StartsWith(Constants.PolicySpecificPrefix))
				{
					continue;
				}

				policyAssignment.Delete(WaitUntil.Completed);
				logger.LogInformation("Deleted policy assignment '{policyAssignmentName}'", policyAssignment.Data.Name);
			}

			group.Delete(WaitUntil.Completed);
			logger.LogInformation("Deleted resource group {resourceGroupName}", group.Data.Name);
		}

		// Cleanup policies
		DeleteAllPolicies(solutionId, environment);
	}

	public bool TryGetAzurePolicyDefinition(string displayName, out SubscriptionPolicyDefinitionResource? policy)
	{
		var subscriptionPolicyDefinitions = subscriptionResource.GetSubscriptionPolicyDefinitions();
		var found = subscriptionPolicyDefinitions.SingleOrDefault(x => x.Data.DisplayName == displayName);

		if (found is null)
		{
			policy = null;
			return false;
		}

		policy = found;
		return true;
	}

	public SubscriptionPolicyDefinitionResource CreateAzureDefinition(PolicyDefinitionData data)
	{
		var policies = subscriptionResource.GetSubscriptionPolicyDefinitions();
		var result = policies.CreateOrUpdate(WaitUntil.Completed, Guid.NewGuid().ToString(), data).Value;
		logger.LogInformation("Created policy {policy}", data.DisplayName);
		return result;
	}


	private ExtendedManagedServiceIdentity? extManagedServiceIdentity;
	public ExtendedManagedServiceIdentity GetManagedIdentity()
	{
		if (extManagedServiceIdentity is not null)
		{
			return extManagedServiceIdentity;
		}

		extManagedServiceIdentity = Retry((attempt) =>
		{
			var resources = subscriptionResource.GetGenericResources($"tagName eq '{Constants.AsmInternalResourceId}' and tagValue eq '{Constants.AsmInternalManagedIdentityResourceIdValue}'");
			foreach (var resource in resources)
			{
				if (resource.Data.Tags.TryGetValue(Constants.AsmInternalSolutionId, out var value))
				{
					if (value == GetASMInternalSolutionIdValue())
					{
						// Data.Properties is null. We have to do a Get so it will be populated in the new res var.
						var res = resource.Get().Value;

						var managedServiceIdentity = new ManagedServiceIdentity(ManagedServiceIdentityType.UserAssigned);
						var userIdentity = res.Data.Properties.ToObjectFromJson<UserAssignedIdentity>();
						managedServiceIdentity.UserAssignedIdentities.Add(resource.Id, userIdentity);
						return new ExtendedManagedServiceIdentity(managedServiceIdentity, resource.Data.Location);
					}
				}
			}

			if (attempt > 1)
			{
				logger.LogDebug("Attempt {attempt} to get managed identity.", attempt);
			}

			return default;
		}, Constants.DefaultRetryCount);

		if (extManagedServiceIdentity is null)
		{
			throw new UserException("Subscription needs to be initialized. Use the init command to initialize.");
		}

		return extManagedServiceIdentity;
	}

	private static T? Retry<T>(Func<int, T?> work, int retryCount) where T : class
	{
		for (var i = 0; i < retryCount; i++)
		{
			var found = work(i + 1);
			if (found is not null)
			{
				return found;
			}

			Thread.Sleep(TimeSpan.FromSeconds(i + 1));
		}

		return default;
	}

	public ExtendedManagedServiceIdentity CreateManagedIdentityIfMissing(string managedIdentityName, string resourceGroupName, AzureLocation location)
	{
		ResourceIdentifier id = new($"/subscriptions/{subscriptionId}/resourcegroups/{resourceGroupName}/providers/Microsoft.ManagedIdentity/userAssignedIdentities/{managedIdentityName}");

		var tags = new Dictionary<string, string>
		{
			{ Constants.AsmInternalSolutionId, GetASMInternalSolutionIdValue() },
			{ Constants.AsmInternalResourceId, Constants.AsmInternalManagedIdentityResourceIdValue }
		};

		UserAssignedIdentity userIdentity;
		var resources = client.GetGenericResources();
		if (!resources.Exists(id))
		{
			var data = new GenericResourceData(location);
			tags.ToList().ForEach(data.Tags.Add);
			var result = resources.CreateOrUpdate(WaitUntil.Completed, id, data);
			logger.LogInformation("Created managed identity {managedIdentity}", id);
			userIdentity = result.Value.Data.Properties.ToObjectFromJson<UserAssignedIdentity>();
		}
		else
		{
			var result = client.GetGenericResource(id).Get().Value;
			if (result.Data.ApplyTagsIfMissingOrTagValueIfDifferent(tags))
			{
				result = resources.CreateOrUpdate(WaitUntil.Completed, id, result.Data).Value;
			}

			userIdentity = result.Data.Properties.ToObjectFromJson<UserAssignedIdentity>();

		}

		var managedIdentity = new ManagedServiceIdentity(ManagedServiceIdentityType.UserAssigned);
		managedIdentity.UserAssignedIdentities.Add(id, userIdentity);
		return new ExtendedManagedServiceIdentity(managedIdentity, location);
	}

	public void ApplyPolicyAssignmentIfNotExist(ResourceGroupResource group, ExtendedManagedServiceIdentity managedIdentity, SubscriptionPolicyDefinitionResource policy)
	{
		var assignments = group.GetPolicyAssignments();
		if (!assignments.Any(x => x.Data.DisplayName == policy.Data.DisplayName))
		{
			var assignment = new PolicyAssignmentData
			{
				PolicyDefinitionId = policy.Id,
				DisplayName = policy.Data.DisplayName,
				ManagedIdentity = managedIdentity.ManagedServiceIdentity,
				Location = group.Data.Location,
			};
			assignments.CreateOrUpdate(WaitUntil.Completed, policy.Data.DisplayName, assignment);
			logger.LogInformation("Created policy assignment for '{policy}' in {resourceGroupName}", policy.Data.DisplayName, group.Data.Name);
		}
	}

	public void ApplyTaggingRoleAssignment(ResourceGroupResource group, ExtendedManagedServiceIdentity managedIdentity)
	{
		var roleDefinitionId = string.Format(Constants.RoleDefinationIds.SubscriptionTagContributor, subscriptionId);
		var roleAssignments = client.GetRoleAssignments(group.Id);

		if (!roleAssignments.Any(x => x.Data.RoleDefinitionId == roleDefinitionId))
		{
			var roleAssignment = new RoleAssignmentCreateOrUpdateContent(
				roleDefinitionId: new ResourceIdentifier(roleDefinitionId), managedIdentity.ManagedServiceIdentity.GetPrincipalId())
			{
				PrincipalType = RoleManagementPrincipalType.ServicePrincipal
			};
			roleAssignments.CreateOrUpdate(WaitUntil.Completed, Guid.NewGuid().ToString(), roleAssignment);

			logger.LogInformation("Created role assignment for {managedIdentity} in {resourceGroupName}",
				managedIdentity.ManagedServiceIdentity.GetPrincipalId(),
				group.Data.Name);
		}
	}

	public bool TryGetResourceGroupResource(string resourceGroupName, out ResourceGroupResource? resourceGroup)
	{
		var resourceGroups = subscriptionResource.GetResourceGroups();
		resourceGroup = resourceGroups.SingleOrDefault(x => x.Id.ResourceGroupName == resourceGroupName);
		if (resourceGroup is null)
		{
			return false;
		}

		return true;
	}

	public ResourceGroupResource CreateResourceGroup(string resourceGroupName, string location, Func<ResourceGroupData, bool> taggingAction)
	{
		var resourceGroups = subscriptionResource.GetResourceGroups();
		var resourceGroupData = new ResourceGroupData(location);
		taggingAction(resourceGroupData);
		var result = resourceGroups.CreateOrUpdate(WaitUntil.Completed, resourceGroupName, resourceGroupData);
		logger.LogInformation("Created resource group {resourceGroupName}", resourceGroupName);
		return result.Value;
	}

	public bool TryUpdateTaggingIfMissing(ResourceGroupResource group, Func<ResourceGroupData, bool> taggingAction)
	{
		if (taggingAction(group.Data))
		{
			var resourceGroupName = group.Data.Name;
			var resourceGroups = subscriptionResource.GetResourceGroups();
			resourceGroups.CreateOrUpdate(WaitUntil.Completed, resourceGroupName, group.Data);
			logger.LogInformation("Updated resource group {resourceGroupName}", resourceGroupName);
			return true;
		}

		return false;
	}

	public void CreateCanNotDeleteLockIfMissing(ResourceGroupResource resourceGroupResource)
	{
		var locks = resourceGroupResource.GetManagementLocks();
		if (locks.Any(x => x.Data.Level == ManagementLockLevel.CanNotDelete))
		{
			return;
		}
		var lockData = new ManagementLockData(ManagementLockLevel.CanNotDelete)
		{
			Notes = Constants.LockNotes
		};
		var lockName = $"{resourceGroupResource.Data.Name}{Constants.LockNameSuffix}";
		locks.CreateOrUpdate(WaitUntil.Completed, lockName, lockData);

		logger.LogInformation("Created lock {lockName} in {resourceGroupName}", lockName, resourceGroupResource.Data.Name);
	}

	public string GetASMInternalSolutionIdValue()
	{
#if DEBUG
		if (options.DevTest)
		{
			return Constants.AsmInternalSolutionIdDevTestValue;
		}
#endif

		return Constants.AsmInternalSolutionIdValue;
	}

	public ResourceGroupResource? GetResourceGroup(string resourceGroupName)
	{
		var resourceGroups = subscriptionResource.GetResourceGroups();
		return resourceGroups.SingleOrDefault(x => x.Data.Name == resourceGroupName);
	}

	public GenericResource? GetResource(string resourceId)
	{
		return Retry((attempt) =>
		{
			var page = subscriptionResource.GetGenericResources($"tagName eq '{Constants.AsmResourceId}' and tagValue eq '{resourceId}'");
			var res = page.SingleOrDefault();

			if (attempt > 1)
			{
				logger.LogDebug("Attempt {attempt} to get resource with id {resourceId}.", attempt, resourceId);
			}

			return res;

		}, Constants.DefaultRetryCount);
	}

	public ResourceIdentifier? GetRoleDefination(string roleName)
	{
		var res = subscriptionResource.GetAuthorizationRoleDefinitions().SingleOrDefault(x => x.Data.RoleName == roleName);
		return res?.Id;
	}

	public bool ApplyResourceGroupUserRole(
		ResourceGroupResource resourceGroupResource,
		ResourceIdentifier roleDefinitionId,
		Guid principalId,
		RoleManagementPrincipalType principalType)
	{
		var roleAssignments = resourceGroupResource.GetRoleAssignments();
		if (!roleAssignments.Any(x => x.Data.RoleDefinitionId == roleDefinitionId && x.Data.PrincipalId == principalId))
		{
			var roleAssignment = new RoleAssignmentCreateOrUpdateContent(roleDefinitionId, principalId)
			{
				PrincipalType = principalType,
			};

			var result = roleAssignments.CreateOrUpdate(WaitUntil.Completed, Guid.NewGuid().ToString(), roleAssignment);

			logger.LogInformation("Created role assignment for {identity} in {resourceGroupName}",
				principalId,
				resourceGroupResource.Data.Name);

			return true;
		}

		logger.LogInformation("Role assignment for {identity} exist in {resourceGroupName}",
			principalId,
			resourceGroupResource.Data.Name);

		return false;
	}

	public GenericResource[]? GetResources(string solutionId, string environment, string resourceType, string? region)
	{
		var group = region is not null ? GetResourceGroup(solutionId, environment, region) : GetResourceGroup(solutionId, environment);
		if (group is not null)
		{
			return Retry((attempt) =>
			{
				var resources = group.GetGenericResources(filter: $"resourceType eq '{resourceType}'").ToArray();

				if (resources is not null)
				{
					return resources;
				}

				if (attempt > 1)
				{
					logger.LogDebug("Attempt {attempt} to get resource type {resourceType}.", attempt, resourceType);
				}

				return default;
			}, Constants.DefaultRetryCount);
		}

		return default;
	}
}
