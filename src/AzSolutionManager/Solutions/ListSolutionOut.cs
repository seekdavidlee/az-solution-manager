namespace AzSolutionManager.Solutions;

public class ListSolutionOut
{
    public ListSolutionOut()
    {
		ResourceGroups = new();
    }

    public string? SolutionId { get; set; }

	public List<ListSolutionResourceGroup> ResourceGroups { get; set; }
}

public class ListSolutionResourceGroup
{
	public string? Environment { get; set; }

	public string? Region { get; set; }

	public string? Name { get; set; }

	public string? Component { get; set; }

	public string? Location { get; set; }
}
