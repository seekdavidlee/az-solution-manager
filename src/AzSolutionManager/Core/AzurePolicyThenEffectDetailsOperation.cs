using System.Text.Json.Serialization;

namespace AzSolutionManager.Core;

public class AzurePolicyThenEffectDetailsOperation
{
    [JsonPropertyName("operation")]
    public string Operation { get; set; }

    [JsonPropertyName("field")]
    public string Field { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }

    public AzurePolicyThenEffectDetailsOperation(string operation, string field, string value)
    {
        Operation = operation;
        Field = field;
        Value = value;
    }
}
