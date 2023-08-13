using AzSolutionManager.Authorization;
using AzSolutionManager.Core;
using AzSolutionManager.Deployment;
using AzSolutionManager.List;
using AzSolutionManager.Lookup;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;
using LogLevel = NLog.LogLevel;

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
		static int listSolutionOptions(ListSolutionOptions options) => options.Run(SetupDependencyInjection(options));

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
			RoleAssignmentOptions,
			ListSolutionOptions>(args).MapResult(
			(Func<InitOptions, int>)initOptions,
			(Func<DeploymentParametersOptions, int>)deploymentParametersOptions,
			(Func<LookupOptions, int>)lookupOptions,
			(Func<ApplyManifestOptions, int>)applyManifestOptions,
			(Func<DestroyOptions, int>)destroyOptions,
			(Func<DestroyAllOptions, int>)destroyAllOptions,
			(Func<RoleAssignmentOptions, int>)roleAssignmentOptions,
			(Func<ListSolutionOptions, int>)listSolutionOptions,
			handleErrors);
	}

	private static ServiceProvider SetupDependencyInjection<T>(T options) where T : class
	{
		var builder = new ConfigurationBuilder()
		  .SetBasePath(Directory.GetCurrentDirectory())
		  .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

		var configuration = builder.Build();

		var logger = LogManager.Setup().LoadConfiguration(builder =>
		{
			var opts = options as BaseOptions;

			if (opts is not null)
			{
				// Create a new NLog configuration
				var config = new LoggingConfiguration();

				// Create a console target
				var consoleTarget = new ConsoleTarget("logconsole")
				{
					StdErr = true,
					Layout = "${MicrosoftConsoleLayout}"
				};

				// Add the console target to the configuration
				config.AddTarget(consoleTarget);

				// Create a rule to route logs to the console target
				var rule = new LoggingRule("*", LogLevel.FromString(opts.LoggingLevel), consoleTarget);

				// Add the rule to the configuration
				config.LoggingRules.Add(rule);

				builder.Configuration = config;
			}

		}).GetCurrentClassLogger();

		var services = new ServiceCollection();
		services.AddSingleton(options);
		services.AddSingleton((IBaseOptions)options);
		services.AddSingleton<AzurePolicyGenerator>();
		services.AddSingleton<ILookupClient, LookupClient>();
		services.AddSingleton<IOneTimeOutWriter, OneTimeOutWriter>();
		services.AddSingleton<IAzureClient, AzureClient>();
		services.AddSingleton<ManifestLoader>();
		services.AddSingleton<IParameterDefinationLoader, ParameterDefinationLoader>();
		services.AddSingleton<ParameterClient>();
		services.AddSingleton<ManifestTokenLookup>();
		services.AddSingleton<RoleAssignmentClient>();
		services.AddSingleton<ListSolutionClient>();
		services.AddLogging(cfg =>
		{
			cfg.ClearProviders();
			cfg.AddNLog();
		});
		services.AddMemoryCache();

		return services.BuildServiceProvider();
	}
}