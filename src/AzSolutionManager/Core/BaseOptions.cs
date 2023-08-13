using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AzSolutionManager.Core;

public abstract class BaseOptions : IBaseOptions
{
	[Option('t', "tenant", HelpText = "Tenant Name or Id.")]
	public string? Tenant { get; set; }

	[Option('s', "subscription", HelpText = "Subscription Name or Id.")]
	public string? Subscription { get; set; }

	/// <summary>
	/// Gets or sets the ASM environment value.
	/// </summary>
	[Option("asm-env", HelpText = "Environment value.")]
	public string? ASMEnvironment { get; set; }

	/// <summary>
	/// Gets or sets the ASM solution value.
	/// </summary>
	[Option("asm-sol", HelpText = "Solution value.")]
	public string? ASMSolutionId { get; set; }

	/// <summary>
	/// Gets or sets the ASM region value.
	/// </summary>
	[Option("asm-reg", HelpText = "Region value.")]
	public string? ASMRegion { get; set; }

	/// <summary>
	/// Gets or sets the ASM resource Id value.
	/// </summary>
	[Option("asm-rid", HelpText = "asm resource Id value.")]
	public string? ASMResourceId { get; set; }

	[Option("logging", HelpText = "Logging levels: Trace, Debug, Info, Warn, Error")]
	public string? LoggingLevel { get; set; } = "Error";

#if DEBUG
	[Option("devtest")]
	public bool DevTest { get; set; }
#endif

	public int Run(ServiceProvider serviceProvider)
	{
		var logger = serviceProvider.GetService<ILogger<Program>>() ?? throw new Exception("Logger cannot be null. This is not expected.");
		try
		{
			logger.LogInformation("Running operation: '{operationName}'", GetOperationName());
			Stopwatch sw = Stopwatch.StartNew();
			RunOperation(serviceProvider);
			sw.Stop();
			logger.LogInformation("Operation '{operationName}' completed in {ms} ms.", GetOperationName(), sw.ElapsedMilliseconds);
			return 0;
		}
		catch (UserException userEx)
		{
			logger.LogError(userEx.Message);
			return -1;
		}
		catch (Exception e)
		{
			logger.LogError(e, "An error has occured while running operation '{operationName}'.", GetOperationName());
			return -1;
		}
	}

	protected abstract string GetOperationName();
	protected abstract void RunOperation(ServiceProvider serviceProvider);
}
