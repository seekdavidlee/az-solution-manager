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
		var verb = this.Value;
		if (verb is null)
		{
			throw new UserException("Missing option input.");
		}

		var lookupClient = serviceProvider.GetRequiredService<ILookupClient>();

		if (verb == Constants.LookupResource)
		{
			if (ASMResourceId is null)
			{
				throw new UserException("Missing --asm-rid");
			}

			if (ASMSolutionId is not null && ASMEnvironment is not null && ASMRegion is not null)
			{
				lookupClient.TryGetUnique(ASMSolutionId, ASMEnvironment, ASMRegion, ASMResourceId);
			}
			else if (ASMSolutionId is not null && ASMEnvironment is not null && ASMRegion is null)
			{
				lookupClient.TryGetUnique(ASMSolutionId, ASMEnvironment, ASMResourceId);
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

			lookupClient.TryGetGroup(ASMSolutionId, ASMEnvironment, ASMRegion);

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

			lookupClient.TryGetByResourceType(ASMSolutionId, ASMEnvironment, ResourceType, ASMRegion);

			return;
		}

		throw new UserException($"Option '{verb}' is invalid.");
	}
}
