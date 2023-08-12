using AzSolutionManager.Authorization;
using AzSolutionManager.Core;
using AzSolutionManager.Deployment;
using AzSolutionManager.Lookup;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;

namespace AzSolutionManager;

public partial class Program
{
    static int Main(string[] args)
    {
        static int initOptions(InitOptions options) => options.Run(SetupDependencyInjection(options));
        static int deploymentParametersOptions(DeploymentParametersOptions options) => options.Run(SetupDependencyInjection(options)); ;
        static int lookupOptions(LookupOptions options) => options.Run(SetupDependencyInjection(options));
        static int applyManifestOptions(ApplyManifestOptions options) => options.Run(SetupDependencyInjection(options));
        static int destroyOptions(DestroyOptions options) => options.Run(SetupDependencyInjection(options));
        static int destroyAllOptions(DestroyAllOptions options) => options.Run(SetupDependencyInjection(options));
        static int roleAssignmentOptions(RoleAssignmentOptions options) => options.Run(SetupDependencyInjection(options));

        static int handleErrors(IEnumerable<Error> errors)
        {
            using TextWriter errorWriter = Console.Error;
            foreach (var er in errors)
            {
                // it seems when performing --version, it shows an error which is not expected.
                if (er.Tag == ErrorType.VersionRequestedError)
                {
                    continue;
                }
                errorWriter.WriteLine(er);
            }

            return -1;
        }

        return Parser.Default.ParseArguments<InitOptions,
            DeploymentParametersOptions,
            LookupOptions,
            ApplyManifestOptions,
            DestroyOptions,
            DestroyAllOptions,
            RoleAssignmentOptions>(args).MapResult(
            (Func<InitOptions, int>)initOptions,
            (Func<DeploymentParametersOptions, int>)deploymentParametersOptions,
            (Func<LookupOptions, int>)lookupOptions,
            (Func<ApplyManifestOptions, int>)applyManifestOptions,
            (Func<DestroyOptions, int>)destroyOptions,
            (Func<DestroyAllOptions, int>)destroyAllOptions,
            (Func<RoleAssignmentOptions, int>)roleAssignmentOptions,
            handleErrors);
    }

    private static ServiceProvider SetupDependencyInjection<T>(T options) where T : class
    {
        var builder = new ConfigurationBuilder()
          .SetBasePath(Directory.GetCurrentDirectory())
          .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        var configuration = builder.Build();

        var logger = LogManager.Setup()
               .LoadConfigurationFromSection(configuration)
               .GetCurrentClassLogger();

        var services = new ServiceCollection();
        services.AddSingleton(options);
        services.AddSingleton((IBaseOptions)options);
        services.AddSingleton<AzurePolicyGenerator>();
        services.AddSingleton<LookupClient>();
        services.AddSingleton<IOneTimeOutWriter, OneTimeOutWriter>();
        services.AddSingleton<IAzureClient, AzureClient>();
        services.AddSingleton<ManifestLoader>();
        services.AddSingleton<ParameterDefinationLoader>();
        services.AddSingleton<ParameterClient>();
        services.AddSingleton<ManifestTokenLookup>();
        services.AddSingleton<RoleAssignmentClient>();
        services.AddLogging(cfg =>
        {
            cfg.ClearProviders();
            cfg.AddNLog();
        });
        services.AddMemoryCache();

        return services.BuildServiceProvider();
    }
}