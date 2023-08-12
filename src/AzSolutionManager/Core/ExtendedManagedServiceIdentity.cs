using Azure.Core;

namespace Azure.ResourceManager.Models;

public class ExtendedManagedServiceIdentity
{
    public ExtendedManagedServiceIdentity(
        ManagedServiceIdentity managedServiceIdentity, AzureLocation location)
    {
		ManagedServiceIdentity = managedServiceIdentity;
		Location = location;
	}

	public ManagedServiceIdentity ManagedServiceIdentity { get; }
	public AzureLocation Location { get; }
}