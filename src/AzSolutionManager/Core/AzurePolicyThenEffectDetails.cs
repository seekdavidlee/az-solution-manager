using System.Text.Json.Serialization;

namespace AzSolutionManager.Core;

public class AzurePolicyThenEffectDetails
{
    public AzurePolicyThenEffectDetails()
    {
        Operations = new();
        RoleDefinationIds = new();
    }

    [JsonPropertyName("operations")]
    public List<AzurePolicyThenEffectDetailsOperation> Operations { get; set; }

    public void AddOrReplaceTag(string key, string value)
    {
        Operations.Add(new AzurePolicyThenEffectDetailsOperation("addOrReplace", $"tags['{key}']", value));
    }

    [JsonPropertyName("roleDefinitionIds")]
    public List<string> RoleDefinationIds { get; set; }
}
