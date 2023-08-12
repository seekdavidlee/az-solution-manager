using AzSolutionManager.Core;
using AzSolutionManager.Lookup;

namespace AzSolutionManager.Deployment;

public class ParameterClient
{
    private readonly LookupClient lookupClient;
    private readonly IOneTimeOutWriter oneTimeOutWriter;
    private readonly ParameterDefinationLoader parameterDefinationLoader;

    public ParameterClient(
        LookupClient lookupClient,
        IOneTimeOutWriter oneTimeOutWriter,
        ParameterDefinationLoader parameterDefinationLoader)
    {
        this.lookupClient = lookupClient;
        this.oneTimeOutWriter = oneTimeOutWriter;
        this.parameterDefinationLoader = parameterDefinationLoader;
    }

    public void CreateDeploymentParameters()
    {
        var d = parameterDefinationLoader.Get();

        if (d.Parameters is null)
        {
            throw new Exception("Parameters must have at least one defination.");
        }

        if (d.SolutionId is null)
        {
            throw new Exception("SolutionId must not be null.");
        }

        if (d.Enviroment is null)
        {
            throw new Exception("Enviroment must not be null.");
        }

        var deploymentOut = new DeploymentOut
        {
            GroupName = lookupClient.GetResourceGroupName(d.SolutionId, d.Enviroment)
        };

        foreach (var p in d.Parameters)
        {
            var h = p.Value.Split(':');

            string? value = null;

            if (h.Length == 1)
            {
                value = p.Value;
            }
            else if (h.Length == 2)
            {
                value = h[0] switch
                {
                    "@env" => Environment.GetEnvironmentVariable(h[1]),
                    "@asm-resource-id" => lookupClient.GetUniqueName(d.SolutionId, d.Enviroment, h[1]),
                    _ => throw new Exception($"{h[0]} is not a valid.")
                };
            }

            if (value is not null)
            {
                deploymentOut.Add(p.Key, value);
            }

            // Do not throw an exception if it is missing. This is expected if resource has not been created yet.
        }

        oneTimeOutWriter.Write(deploymentOut, d.CompressJsonOutput);
    }
}
