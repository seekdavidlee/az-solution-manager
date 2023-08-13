using System.Text.Json.Serialization;

namespace AzSolutionManager.Deployment;

public class ParameterDefination
{
	[JsonPropertyName("parameters")]
	public Dictionary<string, string>? Parameters { get; set; }

	[JsonPropertyName("environment")]
	public string? Enviroment { get; set; }

	[JsonPropertyName("solutionId")]
	public string? SolutionId { get; set; }

	[JsonPropertyName("region")]
	public string? Region { get; set; }

	[JsonPropertyName("compressJsonOutput")]
	public bool CompressJsonOutput { get; set; }
}
