using Automation.SharedKernel.Abstractions.Auth;

namespace Automation.Pipeline.Constants;

public class PipelinePermissions
{
    public static PipelineFeature Pipeline { get; } = new();

    public Dictionary<string, IReadOnlyList<string>> GetPermissions() => new()
    {
        { "Pipeline", Pipeline.All }
    };

    public class PipelineFeature() : BaseCrudPermission("pipeline") { }
}
