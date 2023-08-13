﻿using Azure.ResourceManager.Resources;
using AzSolutionManager.Core;
using Microsoft.Extensions.Logging;

namespace AzSolutionManager.Lookup;

public class LookupClient : ILookupClient
{
	private readonly IAzureClient azureClient;
	private readonly IOneTimeOutWriter oneTimeOutWriter;
	private readonly ILogger<LookupClient> logger;

	public LookupClient(IAzureClient azureClient, IOneTimeOutWriter oneTimeOutWriter, ILogger<LookupClient> logger)
	{
		this.azureClient = azureClient;
		this.oneTimeOutWriter = oneTimeOutWriter;
		this.logger = logger;
	}

	public bool TryGetByResourceType(string solutionId, string environment, string resourceType, string? region)
	{
		var resources = azureClient.GetResources(solutionId, environment, resourceType, region);
		if (resources is not null && resources.Length > 0)
		{
			oneTimeOutWriter.Write(resources.Select(res => new LookupResourceOut(
				resourceId: res.Id.ToString(),
				name: res.Data.Name,
				groupName: res.GetGroupName())).ToArray());
			return true;
		}

		logger.LogWarning("Resource [{resourceType},{solutionId},{environment},{region}] cannot be found.",
			resourceType, solutionId, environment, region is null ? "NoSet" : region);

		return false;
	}

	public string? GetNameByResourceType(string solutionId, string environment, string resourceType, string? region)
	{
		var resources = azureClient.GetResources(solutionId, environment, resourceType, region);
		if (resources is not null && resources.Length == 1)
		{
			return resources.Single().Data.Name;
		}

		return default;
	}

	public bool TryGetGroup(string solutionId, string environment, string? region)
	{
		var group = region is null ?
			azureClient.GetResourceGroup(solutionId, environment) :
			azureClient.GetResourceGroup(solutionId, environment, region);

		if (group is not null)
		{
			oneTimeOutWriter.Write(new LookupGroupOut(groupId: group.Id.ToString(), name: group.Data.Name));
			return true;
		}

		logger.LogWarning("Group [{solutionId},{environment},{region}] cannot be found.",
			solutionId, environment, region is null ? "NoSet" : region);

		return false;
	}

	public bool TryGetUnique(string resourceId)
	{
		var res = azureClient.GetResource(resourceId);
		if (res is not null && res.Id.ResourceGroupName is not null)
		{
			oneTimeOutWriter.Write(new LookupResourceOut(resourceId: res.Id.ToString(), name: res.Data.Name, groupName: res.Id.ResourceGroupName));
			return true;
		}

		logger.LogWarning("Resource [{resourceId}] cannot be found.", resourceId);
		return false;
	}

	public bool TryGetUnique(string solutionId, string environment, string region, string resourceId)
	{
		var group = azureClient.GetResourceGroup(solutionId, environment, region);
		if (group is not null)
		{
			return GetLookup(group, resourceId);
		}

		return false;
	}

	public bool TryGetUnique(string solutionId, string environment, string resourceId)
	{
		var group = azureClient.GetResourceGroup(solutionId, environment);
		if (group is not null)
		{
			return GetLookup(group, resourceId);
		}

		return false;
	}

	public string? GetResourceGroupName(string solutionId, string environment)
	{
		var group = azureClient.GetResourceGroup(solutionId, environment);
		if (group is not null)
		{
			return group.Data.Name;
		}

		return default;
	}

	public string? GetUniqueName(string solutionId, string environment, string resourceId, string? region)
	{
		var group = region is not null ?
			azureClient.GetResourceGroup(solutionId, environment, region) :
			azureClient.GetResourceGroup(solutionId, environment);
		if (group is not null)
		{
			return GetName(group, resourceId);
		}

		return default;
	}

	private bool GetLookup(ResourceGroupResource group, string resourceId)
	{
		var resources = group.GetGenericResources($"tagName eq '{Constants.AsmResourceId}' and tagValue eq '{resourceId}'");
		var res = resources.FirstOrDefault();
		if (res is not null)
		{
			oneTimeOutWriter.Write(new LookupResourceOut(resourceId: res.Id.ToString(), name: res.Data.Name, groupName: group.Data.Name));
			return true;
		}

		logger.LogWarning("Resource [{resourceId}] cannot be found in group [{group}]", resourceId, group.Data.Name);
		return false;
	}

	private static string? GetName(ResourceGroupResource group, string resourceId)
	{
		var resources = group.GetGenericResources($"tagName eq '{Constants.AsmResourceId}' and tagValue eq '{resourceId}'");
		var res = resources.FirstOrDefault();
		if (res is not null)
		{
			return res.Data.Name;
		}

		return default;
	}
}
