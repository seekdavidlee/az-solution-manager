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

    public void ListSolutions(string? regionFilter, string? environmentFilter, string? solutionIdFilter, string? componentFilter)
    {
        var solutions = azureClient.GetSolutions()?.Where(x => x.Data.Tags.ContainsKey(Constants.AsmSolutionId));
        if (solutions is not null && solutions.Any())
        {
            Dictionary<string, ListSolutionOut> results = new();
            foreach (var solution in solutions)
            {
                if (regionFilter is not null && !solution.Data.Tags.ContainsKey(Constants.AsmRegion))
                {
                    continue;
                }

                if (regionFilter is not null && solution.Data.Tags.TryGetValue(Constants.AsmRegion, out string? valueReg) && valueReg != regionFilter)
                {
                    continue;
                }

                if (environmentFilter is not null && !solution.Data.Tags.ContainsKey(Constants.AsmEnvironment))
                {
                    continue;
                }

                if (environmentFilter is not null && solution.Data.Tags.TryGetValue(Constants.AsmEnvironment, out string? valueEnv) && valueEnv != environmentFilter)
                {
                    continue;
                }

                if (solutionIdFilter is not null && !solution.Data.Tags.ContainsKey(Constants.AsmSolutionId))
                {
                    continue;
                }

                if (solutionIdFilter is not null && solution.Data.Tags.TryGetValue(Constants.AsmSolutionId, out string? valueSol) && valueSol != solutionIdFilter)
                {
                    continue;
                }

                if (componentFilter is not null && !solution.Data.Tags.ContainsKey(Constants.AsmComponent))
                {
                    continue;
                }

                if (componentFilter is not null && solution.Data.Tags.TryGetValue(Constants.AsmComponent, out string? valueCom) && valueCom != componentFilter)
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
                    var newVal = new ListSolutionOut
                    {
                        SolutionId = solutionId
                    };
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
