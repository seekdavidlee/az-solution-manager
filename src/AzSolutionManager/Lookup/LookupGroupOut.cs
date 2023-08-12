namespace AzSolutionManager.Lookup;

public class LookupGroupOut
{
    public LookupGroupOut(string groupId, string name)
    {
        GroupId = groupId;
        Name = name;
    }

    public string GroupId { get; set; }

    public string Name { get; set; }
}