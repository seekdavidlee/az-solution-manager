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
		var solutions = azureClient.GetSolutions()?.Where(x => x.Data.Tags.ContainsKey(Constants.AsmSolutionId));
		if (solutions is not null && solutions.Any())
		{
			Dictionary<string, ListSolutionOut> results = new();
			foreach (var solution in solutions)
			{
				if (region is not null && solution.Data.Tags.ContainsKey(Constants.AsmRegion) &&
					solution.Data.Tags[Constants.AsmRegion] != region)
				{
					continue;
				}

				if (environment is not null && solution.Data.Tags.ContainsKey(Constants.AsmEnvironment) &&
					solution.Data.Tags[Constants.AsmEnvironment] != environment)
				{
					continue;
				}

				var solutionId = solution.Data.Tags[Constants.AsmSolutionId];
				var mapped = new ListSolutionResourceGroup
				{
					Environment = solution.Data.Tags[Constants.AsmEnvironment]?.ToString(),
					Region = solution.Data.Tags.ContainsKey(Constants.AsmRegion) ? solution.Data.Tags[Constants.AsmRegion] : null,
					Component = solution.Data.Tags.ContainsKey(Constants.AsmComponent) ? solution.Data.Tags[Constants.AsmComponent] : null,
					Name = solution.Data.Name,
					Location = solution.Data.Location
				};

				if (results.TryGetValue(solutionId, out ListSolutionOut? val))
				{
					if (val is null)
					{
						throw new Exception("ListSolutionOut out Value is null");
					}

					val.ResourceGroups.Add(mapped);
				} 
				else
				{
					var newVal = new ListSolutionOut();
					newVal.SolutionId = solutionId;
					newVal.ResourceGroups.Add(mapped);
					results.Add(solutionId, newVal);
				}
			}

			oneTimeOutWriter.Write(results.Values.ToArray());
		}
		else
		{
			logger.LogDebug("No solutions found in subscription.");
		}
	}
}
