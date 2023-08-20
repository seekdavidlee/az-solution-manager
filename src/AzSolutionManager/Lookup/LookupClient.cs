using Azure.ResourceManager.Resources;
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

	public bool TryGetByResourceType(string solutionId, string environment, string resourceType, string? region, string? component)
	{
		var resources = azureClient.GetResources(solutionId, environment, resourceType, region, component);
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

	public string? GetNameByResourceType(string solutionId, string environment, string resourceType, string? region, string? component)
	{
		var resources = azureClient.GetResources(solutionId, environment, resourceType, region, component);
		if (resources is not null && resources.Length == 1)
		{
			return resources.Single().Data.Name;
		}

		return default;
	}

	public bool TryGetGroups(string solutionId, string environment, string? region, string? component)
	{
		var groups = azureClient.GetResourceGroups(solutionId, environment, region, component).ToArray();

		if (groups is not null && groups.Length > 0)
		{
			oneTimeOutWriter.Write(
				groups.Select(x =>
				new LookupGroupOut(
					groupId: x.Id.ToString(),
					name: x.Data.Name)
				{
					Component = x.Data.Tags.ContainsKey(Constants.AsmComponent) ? x.Data.Tags[Constants.AsmComponent] : default
				}).ToArray());

			return true;
		}

		logger.LogWarning("Group [{solutionId},{environment}, region: {region}, component: {component}] cannot be found.",
			solutionId, environment, region is null ? "NoSet" : region, component is null ? "NotSet" : component);

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

	public bool TryGetUnique(string resourceId, string solutionId, string environment, string? region, string? component)
	{
		var groups = azureClient.GetResourceGroups(solutionId, environment, region: region, component: component);
		foreach (var group in groups)
		{
			if (GetLookup(group, resourceId))
			{
				return true;
			}
		}

		return false;
	}

	public string? GetResourceGroupName(string solutionId, string environment, string? component)
	{
		var groups = azureClient.GetResourceGroups(solutionId, environment, region: null, component: component);
		if (groups is not null)
		{
			var found = groups.Count();
			if (found > 1)
			{
				throw new UserException("More than one group was found. Please provide component.");
			}

			if (found == 1)
			{
				return groups.Single().Data.Name;
			}
		}

		return default;
	}

	public string? GetUniqueName(string solutionId, string environment, string resourceId, string? region, string? component)
	{
		var groups = azureClient.GetResourceGroups(solutionId, environment, region, component);
		foreach (var group in groups)
		{
			var name = GetName(group, resourceId);
			if (name is not null)
			{
				return name;
			}
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

		logger.LogDebug("Resource [{resourceId}] cannot be found in group [{group}]", resourceId, group.Data.Name);
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
