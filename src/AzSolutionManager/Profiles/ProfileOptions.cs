using AzSolutionManager.Core;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace AzSolutionManager.Profiles;

[Verb("profile", HelpText = "Manage profile.")]
public class ProfileOptions : BaseOptions
{
    [Option("show")]
    public bool Show { get; set; }

    [Option("clear")]
    public bool Clear { get; set; }

    private const string operationName = "Profile";

    protected override string GetOperationName()
    {
        return operationName;
    }

    protected override void RunOperation(ServiceProvider serviceProvider)
    {
        if (Clear)
        {
            ProfileClient.Delete();
            return;
        }

        if (Show)
        {
            var p = ProfileClient.Get();

            if (p is not null)
            {
                Console.Out.WriteLine(JsonSerializer.Serialize(p));
            }

            return;
        }

        if (this.Subscription is null)
        {
            throw new UserException("Missing subscription.");
        }

        if (this.Tenant is null)
        {
            throw new UserException("Missing tenant.");
        }

        var profileClient = serviceProvider.GetRequiredService<ProfileClient>();
        profileClient.Save(this.Subscription, this.Tenant);
    }
}
