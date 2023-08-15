using System.Text.Json.Serialization;
using AzSolutionManager.Core;

namespace AzSolutionManager.Manifests;

public class Manifest
{
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("unique-resource-variables")]
    public Dictionary<string, List<UniqueResource>>? UniqueResourceVariables { get; set; }


    [JsonPropertyName("groups")]
    public List<GroupResource>? Groups { get; set; }

    public void Validate()
    {
        if (Groups is not null)
        {
            HashSet<string> uniqueElements = new();
            foreach (var item in Groups)
            {
                string? name = item.ResourceGroupName;
                if (name is not null && !uniqueElements.Add(name))
                {
                    throw new Exception($"Resource group name {name} cannot be duplicated in the manifest.");
                }
            }
        }
    }
}
