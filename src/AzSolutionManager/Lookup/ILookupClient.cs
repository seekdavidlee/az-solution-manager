namespace AzSolutionManager.Lookup;

public interface ILookupClient
{
	bool TryGetByResourceType(string solutionId, string environment, string resourceType, string? region, string? component);
	string? GetNameByResourceType(string solutionId, string environment, string resourceType, string? region, string? component);
	bool TryGetGroups(string solutionId, string environment, string? region, string? component);
	bool TryGetUnique(string resourceId);
	bool TryGetUnique(string resourceId, string solutionId, string environment, string? region, string? component);
	string? GetResourceGroupName(string solutionId, string environment, string? component);
	string? GetUniqueName(string solutionId, string environment, string resourceId, string? region, string? component);
}
