using AzSolutionManager.Manifests;
using Azure;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.Resources;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;

namespace AzSolutionManager.Core;

public static class Extensions
{

	public static SubscriptionPolicyDefinitionResource CreatePolicy(this IAzureClient azureClient, AzurePolicy azurePolicy, string displayName, string solutionId, string environment, Manifest manifest)
	{
		PolicyDefinitionData data = new()
		{
			Description = displayName,
			DisplayName = displayName,
			PolicyRule = BinaryData.FromObjectAsJson(azurePolicy)
		};

		if (string.IsNullOrEmpty(manifest.Version))
		{
			throw new Exception("Manifest must contain a version that can be applied to the created policy!");
		}

		// See: https://learn.microsoft.com/en-us/azure/governance/policy/concepts/definition-structure#metadata
		Dictionary<string, string> meta = new()
		{
			{ Constants.MetaDataVersionKey, manifest.Version },
			{
			  Constants.MetaDataCategoryKey, !string.IsNullOrEmpty(manifest.Category) ? manifest.Category : Constants.AsmCategory
			},
			{
			  Constants.AsmInternalSolutionId, azureClient.GetASMInternalSolutionIdValue()
			},
			{
			  Constants.AsmSolutionId, solutionId
			},
			{
			  Constants.AsmEnvironment, environment
			}
		};

		data.Metadata = BinaryData.FromObjectAsJson(meta);

		return azureClient.CreateAzureDefinition(data);
	}

	public static bool UpdateNewerVersion(
		this SubscriptionPolicyDefinitionResource subscriptionPolicyDefinition,
		Func<BinaryData> func, Manifest manifest)
	{
		if (manifest.Version is null)
		{
			throw new Exception("Unexpected for manifest.Version to be null.");
		}

		var meta = subscriptionPolicyDefinition.Data.Metadata.ToObjectFromJson<Dictionary<string, string>>();
		if (meta["version"] != manifest.Version)
		{
			meta["version"] = manifest.Version;
			subscriptionPolicyDefinition.Data.Metadata = BinaryData.FromObjectAsJson(meta);
			subscriptionPolicyDefinition.Data.PolicyRule = func();
			subscriptionPolicyDefinition.Update(WaitUntil.Completed, subscriptionPolicyDefinition.Data);
			return true;
		}

		return false;
	}

	public static AzurePolicy CreateAzurePolicy(this UniqueResource uniqueResource)
	{
		if (string.IsNullOrEmpty(uniqueResource.ResourceType)) throw new ApplicationException("ResourceType cannot be null.");
		if (string.IsNullOrEmpty(uniqueResource.ResourceId)) throw new ApplicationException("ResourceId cannot be null.");

		AzurePolicy azurePolicy = new();

		azurePolicy.If.UniqueResource(
			uniqueResource.ResourceType,
			UniqueResource.TagKey,
			uniqueResource.ResourceId);

		if (string.IsNullOrEmpty(uniqueResource.ResourceId)) throw new ApplicationException("ResourceId cannot be null!");

		azurePolicy.ThenEffectModify.Details.AddOrReplaceTag(Constants.AsmResourceId, uniqueResource.ResourceId);

		azurePolicy.ThenEffectModify.Details.RoleDefinationIds.Add(Constants.RoleDefinationIds.TagContributor);

		return azurePolicy;
	}

	public static AzurePolicy CreateAzurePolicy(
		this GroupResource groupResource,
		ManifestTokenLookup manifestTokenLookup)
	{
		string solutionId = groupResource.GetValue(x => x.SolutionId, manifestTokenLookup);
		string env = groupResource.GetValue(x => x.Environment, manifestTokenLookup);


		AzurePolicy azurePolicy = new();

		var dic = new Dictionary<string, string>
		{
			[Constants.AsmSolutionId] = solutionId,
			[Constants.AsmEnvironment] = env
		};

		azurePolicy.ThenEffectModify.Details.AddOrReplaceTag(Constants.AsmSolutionId, solutionId);
		azurePolicy.ThenEffectModify.Details.AddOrReplaceTag(Constants.AsmEnvironment, env);

		if (!string.IsNullOrEmpty(groupResource.Region))
		{
			string region = groupResource.GetValue(x => x.Region, manifestTokenLookup);
			dic[Constants.AsmRegion] = region;
			azurePolicy.ThenEffectModify.Details.AddOrReplaceTag(Constants.AsmRegion, region);
		}

		azurePolicy.If.AnyResource(dic);

		azurePolicy.ThenEffectModify.Details.RoleDefinationIds.Add(Constants.RoleDefinationIds.TagContributor);

		return azurePolicy;
	}

	public static bool ApplyTagsIfMissingOrTagValueIfDifferent(this ResourceGroupData resourceGroupData, Dictionary<string, string> tags)
	{
		bool createOrUpdate = false;
		foreach (var key in tags.Keys)
		{
			if (resourceGroupData.Tags.TryGetValue(key, out string? tagValue))
			{
				if (tagValue != tags[key])
				{
					resourceGroupData.Tags[key] = tagValue;
					createOrUpdate = true;
				}
			}
			else
			{
				resourceGroupData.Tags.Add(key, tags[key]);
				createOrUpdate = true;
			}
		}

		return createOrUpdate;
	}

	public static bool ApplyTagsIfMissingOrTagValueIfDifferent(this GenericResourceData genericResourceData, Dictionary<string, string> tags)
	{
		bool createOrUpdate = false;
		foreach (var key in tags.Keys)
		{
			if (genericResourceData.Tags.TryGetValue(key, out string? tagValue))
			{
				if (tagValue != tags[key])
				{
					genericResourceData.Tags[key] = tagValue;
					createOrUpdate = true;
				}
			}
			else
			{
				genericResourceData.Tags.Add(key, tags[key]);
				createOrUpdate = true;
			}
		}

		return createOrUpdate;
	}

	public static Guid GetPrincipalId(this ManagedServiceIdentity managedServiceIdentity)
	{
		if (managedServiceIdentity.PrincipalId is null)
		{
			if (managedServiceIdentity.ManagedServiceIdentityType == ManagedServiceIdentityType.UserAssigned &&
				managedServiceIdentity.UserAssignedIdentities.Count > 0)
			{
				var userIdentity = managedServiceIdentity.UserAssignedIdentities.First().Value;
				if (userIdentity.PrincipalId is null)
				{
					throw new Exception("Unexpect userIdentity.PrincipalId to be null.");
				}

				return userIdentity.PrincipalId.Value;
			}

			throw new Exception("Unexpect UserAssignedIdentities to be empty.");
		}

		if (managedServiceIdentity.PrincipalId is null)
		{
			throw new Exception("Unexpect managedServiceIdentity.PrincipalId to be null.");
		}

		return managedServiceIdentity.PrincipalId.Value;
	}

	public static Dictionary<string, string> GetTags(this GroupResource groupResource)
	{
		var tags = new Dictionary<string, string>();
		if (groupResource.Region is not null)
		{
			tags.Add(Constants.AsmRegion, groupResource.Region);
		}

		if (groupResource.SolutionId is null)
		{
			throw new Exception("SolutionId in manifest cannot be null.");
		}

		if (groupResource.Environment is null)
		{
			throw new Exception("Environment in manifest cannot be null.");
		}

		tags.Add(Constants.AsmSolutionId, groupResource.SolutionId);
		tags.Add(Constants.AsmEnvironment, groupResource.Environment);

		return tags;
	}

	public static (string resourceGroupName, string solutionId, string environment) ApplyTokens(this GroupResource group, ManifestTokenLookup manifestTokenLookup)
	{
		if (group.ResourceGroupName is null) throw new Exception("ResourceGroupName cannot be null!");
		if (group.Environment is null) throw new Exception("Environment cannot be null!");
		if (group.SolutionId is null) throw new Exception("SolutionId cannot be null!");

		group.ResourceGroupName = manifestTokenLookup.Replace(group.ResourceGroupName);
		group.Environment = manifestTokenLookup.Replace(group.Environment);
		group.SolutionId = manifestTokenLookup.Replace(group.SolutionId);

		if (group.Region is not null)
		{
			group.Region = manifestTokenLookup.Replace(group.Region);
		}

		return (group.ResourceGroupName, group.SolutionId, group.Environment);
	}

	public static string GetValue<T>(this GroupResource group,
		Func<GroupResource, T> selector,
		ManifestTokenLookup manifestTokenLookup)
	{
		T val = selector(group);
		if (val is not null)
		{
			var str = val as string;
			if (str is not null)
			{
				return manifestTokenLookup.Replace(str);
			}
		}

		throw new Exception("");
	}

	public static Dictionary<string, string>? ToDictionary(this BinaryData binaryData)
	{
		using (var stream = new MemoryStream(binaryData.ToArray()))
			return JsonSerializer.Deserialize<Dictionary<string, string>>(stream);
	}

	public static string GetGroupName(this GenericResource resource)
	{
		if (resource.Id.ResourceGroupName == null)
		{
			throw new Exception("Id.ResourceGroupName cannot be null.");
		}

		return resource.Id.ResourceGroupName;
	}
}
