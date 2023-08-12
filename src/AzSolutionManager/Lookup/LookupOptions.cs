using AzSolutionManager.Core;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace AzSolutionManager.Lookup;

[Verb("lookup", HelpText = "Lookup resource or group.")]
public class LookupOptions : BaseOptions
{
	[Option("type", Required = true, HelpText = "'group', 'resource' or 'resource-type'")]
	public string? LookupType { get; set; }

	[Option("type-name", Required = false, HelpText = "The type name of resource.")]
	public string? ResourceType { get; set; }

	protected override string GetOperationName()
	{
		return "Lookup";
	}

	protected override void RunOperation(ServiceProvider serviceProvider)
	{
		var lookupClient = serviceProvider.GetRequiredService<LookupClient>();
		if (LookupType is null)
		{
			throw new Exception("LookupType cannot be null.");
		}

		if (LookupType == Constants.LookupResource)
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

		if (LookupType == Constants.LookupGroup)
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

		if (LookupType == Constants.LookupType)
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
	}
}
