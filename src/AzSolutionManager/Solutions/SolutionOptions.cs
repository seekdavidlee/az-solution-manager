using AzSolutionManager.Core;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace AzSolutionManager.Solutions;

[Verb("solution", HelpText = "Manage solutions in Azure Subscription.")]
public class SolutionOptions : BaseOptions
{
	[Value(0, HelpText = "Valid option(s): list, delete")]
	public string? Value { get; set; }

	private const string operationName = "Solution";

	protected override string GetOperationName()
	{
		return operationName;
	}

	protected override void RunOperation(ServiceProvider serviceProvider)
	{
		var verb = this.Value;
		if (verb is null)
		{
			throw new UserException("Missing option input.");
		}

		if (verb == "list")
		{
			var svc = serviceProvider.GetRequiredService<SolutionClient>();
			svc.ListSolutions(ASMRegion, ASMEnvironment);
			return;
		}

		if (verb == "delete")
		{
			if (ASMSolutionId is null)
			{
				throw new UserException("Missing --asm-sol");
			}

			if (ASMEnvironment is null)
			{
				throw new UserException("Missing --asm-env");
			}

			var svc = serviceProvider.GetRequiredService<AzurePolicyGenerator>();
			svc.Destory(ASMSolutionId, ASMEnvironment);
			return;
		}

		throw new UserException($"Option '{verb}' is invalid.");
	}
}
