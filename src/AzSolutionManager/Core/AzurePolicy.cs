using System.Text.Json.Serialization;

namespace AzSolutionManager.Core;

public class AzurePolicy
{
    public AzurePolicy()
    {
        If = new AzurePolicyDtoIf();
        ThenEffectModify = new AzurePolicyThenEffectModify();
    }

    [JsonPropertyName("if")]
    public AzurePolicyDtoIf If { get; }

    [JsonPropertyName("then")]
    public AzurePolicyThenEffectModify ThenEffectModify { get; }
}
