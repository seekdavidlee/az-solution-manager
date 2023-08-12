using Azure.Core;
using Azure.ResourceManager.Authorization.Models;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.Resources;

namespace AzSolutionManager.Core;

public interface IAzureClient
{
    ExtendedManagedServiceIdentity GetManagedIdentity();
    bool TryGetAzurePolicyDefinition(string displayName, out SubscriptionPolicyDefinitionResource? policy);
    bool TryGetResourceGroupResource(string resourceGroupName, out ResourceGroupResource? resourceGroup);
    bool TryUpdateTaggingIfMissing(ResourceGroupResource group, Func<ResourceGroupData, bool> taggingAction);
    SubscriptionPolicyDefinitionResource CreateAzureDefinition(PolicyDefinitionData data);
    ExtendedManagedServiceIdentity CreateManagedIdentityIfMissing(string managedIdentityName, string resourceGroupName, AzureLocation location);
    void ApplyPolicyAssignmentIfNotExist(ResourceGroupResource group, ExtendedManagedServiceIdentity managedIdentityDetail, SubscriptionPolicyDefinitionResource policy);
    ResourceGroupResource CreateResourceGroup(string resourceGroupName, string location, Func<ResourceGroupData, bool> taggingAction);
    void CreateCanNotDeleteLockIfMissing(ResourceGroupResource resourceGroupResource);
    void ApplyTaggingRoleAssignment(ResourceGroupResource group, ExtendedManagedServiceIdentity managedIdentity);
    void DeleteAllResourceGroups();
    void DeleteResourceGroupsAndPolicies(string solutionId, string environment);
    void DeleteAllPolicies();
    string GetASMInternalSolutionIdValue();
    ResourceGroupResource? GetResourceGroup(string solutionId, string environment);
    ResourceGroupResource? GetResourceGroup(string solutionId, string environment, string region);
    ResourceGroupResource? GetResourceGroup(string resourceGroupName);
    GenericResource? GetResource(string resourceId);
    GenericResource[]? GetResources(string solutionId, string environment, string resourceType, string? region);
	ResourceIdentifier? GetRoleDefination(string roleName);
    public bool ApplyResourceGroupUserRole(ResourceGroupResource resourceGroupResource, ResourceIdentifier roleDefinitionId, Guid principalId, RoleManagementPrincipalType principalType);
}
