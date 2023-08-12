namespace AzSolutionManager.Core;

public static class Constants
{
	public const string AsmInternalSolutionIdValue = "asm";

#if DEBUG
	public const string AsmInternalSolutionIdDevTestValue = "asmdevtest";
#endif

	public const string AsmInternalManagedIdentityResourceIdValue = "asm-internal-managed-identity";

	public const string AsmResourceId = "asm-resource-id";
	public const string AsmCategory = "Azure Resource Discovery";
	public const string AsmInternalSolutionId = "asm-internal-solution-id";
	public const string AsmInternalResourceId = "asm-internal-resource-id";
	public const string AsmSolutionId = "asm-solution-id";
	public const string AsmEnvironment = "asm-environment";
	public const string AsmRegion = "asm-region";
	public const string LockNameSuffix = "-asm-lock";
	public const string LockNotes = "This resource group is locked to prevent accidental deletion.";
	public const string PolicySpecificPrefix = "Enforce asm specific";
	public const string SubscriptionsPrefix = "/subscriptions/{0}";
	public const string LookupResource = "resource";
	public const string LookupGroup = "group";
	public const string LookupType = "resource-type";
	public const string MetaDataVersionKey = "version";
	public const string MetaDataCategoryKey = "category";

	public static class RoleDefinationIds
	{
		public const string TagContributor = "/providers/microsoft.authorization/roleDefinitions/4a9ae827-6dc8-4573-8ac7-8239d42aa03f";
		public const string SubscriptionTagContributor = "/subscriptions/{0}/providers/Microsoft.Authorization/roleDefinitions/4a9ae827-6dc8-4573-8ac7-8239d42aa03f";
	}

	public const int DefaultRetryCount = 10;
}
