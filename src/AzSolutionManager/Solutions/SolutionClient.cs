using AzSolutionManager.Core;
using Microsoft.Extensions.Logging;

namespace AzSolutionManager.Solutions;

public class SolutionClient
{
	private readonly IOneTimeOutWriter oneTimeOutWriter;
	private readonly IAzureClient azureClient;
	private readonly ILogger<SolutionClient> logger;

	public SolutionClient(IOneTimeOutWriter oneTimeOutWriter, IAzureClient azureClient, ILogger<SolutionClient> logger)
	{
		this.oneTimeOutWriter = oneTimeOutWriter;
		this.azureClient = azureClient;
		this.logger = logger;
	}

	public void ListSolutions(string? region, string? environment)
	{
		var solutions = azureClient.GetSolutions();
		if (solutions is not null && solutions.Length > 0)
		{
			var results = solutions.Where(x => x.Data.Tags.ContainsKey(Constants.AsmSolutionId)).Select(x => new ListSolutionOut
			{
				Environment = x.Data.Tags[Constants.AsmEnvironment]?.ToString(),
				Region = x.Data.Tags.ContainsKey(Constants.AsmRegion) ? x.Data.Tags[Constants.AsmRegion] : null,
				SolutionId = x.Data.Tags[Constants.AsmSolutionId]?.ToString(),
				ResourceGroupName = x.Data.Name,
				Location = x.Data.Location
			});

			if (region is not null)
			{
				results = results.Where(x => x.Region == region);
			}

			if (environment is not null)
			{
				results = results.Where(x => x.Environment == environment);
			}

			oneTimeOutWriter.Write(results.ToArray());
		}
		else
		{
			logger.LogDebug("No solutions found in subscription.");
		}
	}
}
