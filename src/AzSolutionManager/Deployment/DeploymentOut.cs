namespace AzSolutionManager.Deployment;

public class DeploymentOut
{
    public string? GroupName { get; set; }

    public Dictionary<string, ValueOut>? Parameters { get; set; }

    public void Add(string key, string value)
    {
        if (Parameters is null)
        {
            Parameters = new();
        }

        Parameters.Add(key, new ValueOut { Value = value });
    }
}
