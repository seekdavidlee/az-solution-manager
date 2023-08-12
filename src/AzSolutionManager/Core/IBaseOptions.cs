namespace AzSolutionManager.Core;

public interface IBaseOptions
{
    string? Tenant { get; set; }

    string? Subscription { get; set; }

    string? ASMEnvironment { get; set; }

    string? ASMSolutionId { get; set; }

    string? ASMRegion { get; set; }

    string? ASMResourceId { get; set; }

#if DEBUG
    bool DevTest { get; set; }
#endif
}
