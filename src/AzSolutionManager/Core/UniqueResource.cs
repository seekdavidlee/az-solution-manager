using System.Text.Json.Serialization;

namespace AzSolutionManager.Core;

public class UniqueResource
{
    [JsonPropertyName("asm-resource-id")]
    public string? ResourceId { get; set; }

    [JsonIgnore]
    public const string TagKey = "asm-resource-id";

    [JsonPropertyName("resource-type")]
    public string? ResourceType { get; set; }
}
