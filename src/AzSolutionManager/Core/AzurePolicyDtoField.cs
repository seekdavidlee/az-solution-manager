using System.Text.Json.Serialization;

namespace AzSolutionManager.Core;

public class AzurePolicyDtoField
{
    [JsonPropertyName("field")]
    public string? Field { get; set; }

    [JsonPropertyName("equals")]
    public string? IsEquals { get; set; }

    [JsonPropertyName("notEquals")]
    public string? IsNotEquals { get; set; }
}
