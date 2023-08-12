using Azure.Core;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace AzSolutionManager.Core;

public class AzurePolicyGenerator
{
	private readonly IAzureClient azureClient;
	private readonly IMemoryCache memoryCache;
	private readonly ManifestTokenLookup manifestTokenLookup;
	private readonly ILogger<AzurePolicyGenerator> logger;

	public AzurePolicyGenerator(IAzureClient azureClient, IMemoryCache memoryCache, ManifestTokenLookup manifestTokenLookup, ILogger<AzurePolicyGenerator> logger)
	{
		this.azureClient = azureClient;
		this.memoryCache = memoryCache;
		this.manifestTokenLookup = manifestTokenLookup;
		this.logger = logger;
	}

	public void Destory(string solutionId, string environment)
	{
		azureClient.DeleteResourceGroupsAndPolicies(solutionId, environment);
	}

	public void Destory()
	{
		azureClient.DeleteAllResourceGroups();
		azureClient.DeleteAllPolicies();
	}

	public void Apply(Manifest manifest)
	{
		ExtendedManagedServiceIdentity managedIdentity = azureClient.GetManagedIdentity();

		if (manifest.Groups is not null)
		{
			foreach (var groupResource in manifest.Groups)
			{
				ProcessGroup(manifest, groupResource, managedIdentity);
			}
		}
	}

	private void ProcessGroup(Manifest manifest, GroupResource groupResource, ExtendedManagedServiceIdentity managedIdentity)
	{
		var (resourceGroupName, solutionId, environment) = groupResource.ApplyTokens(manifestTokenLookup);

		if (manifest is null)
		{
			throw new Exception("Unexpected for manifest to be null.");
		}
		string displayName = $"{Constants.PolicySpecificPrefix} solution tags for {groupResource.ResourceGroupName}";

		if (!azureClient.TryGetAzurePolicyDefinition(displayName, out SubscriptionPolicyDefinitionResource? policy))
		{
			policy = azureClient.CreatePolicy(groupResource.CreateAzurePolicy(manifestTokenLookup), displayName, solutionId, environment, manifest);
		}
		else
		{
			if (policy is null)
			{
				throw new Exception("Unexpected for policy to be null.");
			}

			if (policy.UpdateNewerVersion(() => BinaryData.FromObjectAsJson(groupResource.CreateAzurePolicy(manifestTokenLookup)), manifest))
			{
				logger.LogInformation("Updated policy '{policyName}'", policy.Data.DisplayName);
			}
		}

		if (groupResource.ResourceGroupName is null)
		{
			throw new Exception("ResourceGroupName cannot be null");
		}

		CreateResourceGroup(
			 resourceGroupName: groupResource.ResourceGroupName,
			 location: groupResource.ResourceGroupLocation ?? managedIdentity.Location,
			 tags: groupResource.GetTags(),
			 managedIdentity: managedIdentity,
			 policy: policy);

		if (groupResource.UniqueResourcesVariableKey is not null &&
				manifest.UniqueResourceVariables is not null)
		{
			var uniqueResources = manifest.UniqueResourceVariables[groupResource.UniqueResourcesVariableKey];
			foreach (var UniqueResource in uniqueResources)
			{
				ProcessUniqueResource(
						manifest: manifest,
						resourceGroupName: groupResource.ResourceGroupName,
						solutionId: solutionId,
						environment: environment,
						uniqueResource: UniqueResource,
						managedIdentity: managedIdentity);
			}
		}
	}

	private void ProcessUniqueResource(Manifest manifest, string resourceGroupName, string solutionId, string environment, UniqueResource uniqueResource, ExtendedManagedServiceIdentity managedIdentity)
	{
		string displayName = $"{Constants.PolicySpecificPrefix} resource tag {uniqueResource.ResourceId}";
		if (!azureClient.TryGetAzurePolicyDefinition(displayName, out SubscriptionPolicyDefinitionResource? policy))
		{
			policy = azureClient.CreatePolicy(uniqueResource.CreateAzurePolicy(), displayName, solutionId, environment, manifest);
		}
		else
		{
			if (policy is null)
			{
				throw new Exception("Unexpected for policy to be null.");
			}

			if (policy.UpdateNewerVersion(() => BinaryData.FromObjectAsJson(uniqueResource.CreateAzurePolicy()), manifest))
			{
				logger.LogInformation("Updated policy '{policyName}'", policy.Data.DisplayName);
			}
		}

		if (memoryCache.Get(GetMemCacheKey(resourceGroupName)) is null)
		{
			throw new Exception($"Unable to assign policy to group {resourceGroupName} as it is not specified in the manifest within group-resources.");
		}

		CreateResourceGroup(
			 resourceGroupName: resourceGroupName,
			 location: managedIdentity.Location,    // This is a placeholder. I don't expect this to be used.
			 tags: null,
			 managedIdentity: managedIdentity,
			 policy: policy);
	}

	public ExtendedManagedServiceIdentity ApplyManagedIdentity(
			string resourceGroupName,
			string managedIdentityName,
			string resourceGroupLocation)
	{
		CreateResourceGroup(
			 resourceGroupName: resourceGroupName,
			 location: resourceGroupLocation,
			 tags: null,
			 managedIdentity: null,
			 policy: null);

		return azureClient.CreateManagedIdentityIfMissing(
				managedIdentityName: managedIdentityName,
				resourceGroupName: resourceGroupName,
				location: new AzureLocation(resourceGroupLocation));
	}

	private void CreateResourceGroup(string resourceGroupName, AzureLocation location, Dictionary<string, string>? tags, ExtendedManagedServiceIdentity? managedIdentity, SubscriptionPolicyDefinitionResource? policy)
	{
		bool taggingAction(ResourceGroupData rg)
		{
			tags ??= new Dictionary<string, string>();
			tags.TryAdd(Constants.AsmInternalSolutionId, azureClient.GetASMInternalSolutionIdValue());
			return rg.ApplyTagsIfMissingOrTagValueIfDifferent(tags);
		}

		string memKey = GetMemCacheKey(resourceGroupName);
		var group = memoryCache.GetOrCreate(memKey, (entry) =>
		{
			if (!azureClient.TryGetResourceGroupResource(resourceGroupName, out var grp))
			{
				grp = azureClient.CreateResourceGroup(resourceGroupName, location, taggingAction);
			}
			else
			{
				if (grp is null)
				{
					throw new Exception("Unexpected grp is null.");
				}

				azureClient.TryUpdateTaggingIfMissing(grp, taggingAction);
			}

			return grp;
		}) ?? throw new Exception("Unexpected group is null.");
		azureClient.CreateCanNotDeleteLockIfMissing(group);

		if (managedIdentity is not null)
		{
			azureClient.ApplyTaggingRoleAssignment(group, managedIdentity);

			if (policy is not null)
			{
				azureClient.ApplyPolicyAssignmentIfNotExist(group, managedIdentity, policy);
			}
		}
	}

	private static string GetMemCacheKey(string resourceGroupName)
	{
		return $"resourceGroup:{resourceGroupName}";
	}
}