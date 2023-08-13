namespace AzSolutionManager.Lookup;

public interface ILookupClient
{
	bool TryGetByResourceType(string solutionId, string environment, string resourceType, string? region);
	string? GetNameByResourceType(string solutionId, string environment, string resourceType, string? region);
	bool TryGetGroup(string solutionId, string environment, string? region);
	bool TryGetUnique(string resourceId);
	bool TryGetUnique(string solutionId, string environment, string region, string resourceId);
	bool TryGetUnique(string solutionId, string environment, string resourceId);
	string? GetResourceGroupName(string solutionId, string environment);
	string? GetUniqueName(string solutionId, string environment, string resourceId, string? region);
}
