using System.Text.Json;

namespace AzSolutionManager.Deployment;

public class ParameterDefinationLoader : IParameterDefinationLoader
{
	private readonly DeploymentOptions options;

	public ParameterDefinationLoader(DeploymentOptions options)
	{
		this.options = options;
	}

	private ParameterDefination? parameterDefination;

	public ParameterDefination Get()
	{
		if (parameterDefination is not null)
		{
			return parameterDefination;
		}

		if (options.FilePath is null)
		{
			throw new Exception("Filepath is required.");
		}

		string content = File.ReadAllText(options.FilePath);

		if (string.IsNullOrEmpty(content))
		{
			throw new Exception("Unexpected for manifest content to be empty.");
		}

		var d = JsonSerializer.Deserialize<ParameterDefination>(content) ?? throw new Exception("Unexpected for manifest null.");
		parameterDefination = d;
		return parameterDefination;
	}
}