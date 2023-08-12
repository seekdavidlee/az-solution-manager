using System.Text.Json.Serialization;

namespace AzSolutionManager.Core;

public class AzurePolicyThenEffectModify
{
    public AzurePolicyThenEffectModify()
    {
        Details = new();
    }

    [JsonPropertyName("effect")]
    public string Effect { get; set; } = "modify";

    [JsonPropertyName("details")]
    public AzurePolicyThenEffectDetails Details { get; set; }
}
