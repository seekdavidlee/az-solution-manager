namespace AzSolutionManager.Lookup;

public class LookupResourceOut
{
    public LookupResourceOut(string resourceId, string name, string groupName)
    {
        ResourceId = resourceId;
        Name = name;
        GroupName = groupName;

    }

    public string ResourceId { get; set; }

    public string Name { get; set; }

    public string GroupName { get; set; }
}
