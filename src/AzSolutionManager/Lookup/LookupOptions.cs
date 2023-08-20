using AzSolutionManager.Core;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace AzSolutionManager.Lookup;

[Verb("lookup", HelpText = "Lookup resource or group.")]
public class LookupOptions : BaseOptions
{
	[Value(0, HelpText = "Valid option(s): group, resource, resource-type")]
	public string? Value { get; set; }

	[Option("type-name", Required = false, HelpText = "The type name of resource.")]
	public string? ResourceType { get; set; }

	private const string operationName = "Lookup";

	protected override string GetOperationName()
	{
		return operationName;
	}

	protected override void RunOperation(ServiceProvider serviceProvider)
	{
		var verb = this.Value ?? throw new UserException("Missing option input.");
		var lookupClient = serviceProvider.GetRequiredService<ILookupClient>();

		if (verb == Constants.LookupResource)
		{
			if (ASMResourceId is null)
			{
				throw new UserException("Missing --asm-rid");
			}

			if (ASMSolutionId is not null && ASMEnvironment is not null)
			{
				lookupClient.TryGetUnique(ASMResourceId, ASMSolutionId, ASMEnvironment, ASMRegion, ASMComponent);
			}
			else
			{
				lookupClient.TryGetUnique(ASMResourceId);
			}

			return;
		}

		if (verb == Constants.LookupGroup)
		{
			if (ASMSolutionId is null)
			{
				throw new UserException("Missing --asm-soln");
			}

			if (ASMEnvironment is null)
			{
				throw new UserException("Missing --asm-env");
			}

			lookupClient.TryGetGroups(ASMSolutionId, ASMEnvironment, ASMRegion, ASMComponent);

			return;
		}

		if (verb == Constants.LookupType)
		{
			if (ASMSolutionId is null)
			{
				throw new UserException("Missing --asm-soln");
			}

			if (ASMEnvironment is null)
			{
				throw new UserException("Missing --asm-env");
			}

			if (ResourceType is null)
			{
				throw new UserException("Missing --resource-type");
			}

			lookupClient.TryGetByResourceType(ASMSolutionId, ASMEnvironment, ResourceType, ASMRegion, ASMComponent);

			return;
		}

		throw new UserException($"Option '{verb}' is invalid.");
	}
}
