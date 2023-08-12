using System.Text.Json.Serialization;

namespace AzSolutionManager.Core;

public class AzurePolicyDtoIf
{
    public AzurePolicyDtoIf()
    {
        AllOf = new List<AzurePolicyDtoField>();
        AnyOf = new List<AzurePolicyDtoField>();
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("allOf")]
    public List<AzurePolicyDtoField>? AllOf { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("anyOf")]
    public List<AzurePolicyDtoField>? AnyOf { get; set; }

    public void AnyResource(Dictionary<string, string> tags)
    {
        AllOf = null;

        if (AnyOf == null) throw new ApplicationException("AnyOf property state is invalid.");

        AnyOf.AddRange(tags.Select((x) => new AzurePolicyDtoField { Field = $"tags['{x.Key}']", IsNotEquals = x.Value }));
    }

    public void UniqueResource(string type, string tagKey, string tagValue)
    {
        AnyOf = null;

        if (AllOf == null) throw new ApplicationException("AllOf property state is invalid.");

        AllOf.AddRange(new[]
        {
            new AzurePolicyDtoField
            {
                Field = "type",
                IsEquals = type
            },
            new AzurePolicyDtoField
            {
                Field = $"tags['{tagKey}']",
                IsNotEquals = tagValue
            }
        });
    }
}
