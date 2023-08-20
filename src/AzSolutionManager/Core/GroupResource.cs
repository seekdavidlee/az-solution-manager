using System.Text.Json.Serialization;

namespace AzSolutionManager.Core;

public class GroupResource
{
    [JsonPropertyName("resource-group-name")]
    public string? ResourceGroupName { get; set; }

    [JsonPropertyName("unique-resources-variable-key")]
    public string? UniqueResourcesVariableKey { get; set; }

    [JsonPropertyName(Constants.AsmSolutionId)]
    public string? SolutionId { get; set; }

    [JsonPropertyName(Constants.AsmEnvironment)]
    public string? Environment { get; set; }

    [JsonPropertyName(Constants.AsmRegion)]
    public string? Region { get; set; }

    [JsonPropertyName("resource-group-location")]
    public string? ResourceGroupLocation { get; set; }

	[JsonPropertyName(Constants.AsmComponent)]
	public string? Component { get; set; }
}
