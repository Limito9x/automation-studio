using Automation.Pipeline.Domain.Enums;

namespace Automation.Pipeline.Domain.Entities;

public class PipelineEdge : AuditableEntity
{
    public Guid PipelineId { get; set; }
    public Pipeline Pipeline { get; set; } = null!;
    public Guid SourcePipelineNodeId { get; set; }
    public PipelineNode SourcePipelineNode { get; set; } = null!;
    public string SourcePin { get; set; } = string.Empty;
    public Guid TargetPipelineNodeId { get; set; }
    public PipelineNode TargetPipelineNode { get; set; } = null!;
    public string TargetPin { get; set; } = string.Empty;
    public EdgeKind Kind { get; set; }

    public PipelineEdge() { }

    public PipelineEdge(
        Guid pipelineId,
        Guid sourcePipelineNodeId,
        string sourcePin,
        Guid targetPipelineNodeId,
        string targetPin,
        EdgeKind? kind = null
    )
    {
        PipelineId = pipelineId;
        SourcePipelineNodeId = sourcePipelineNodeId;
        SourcePin = sourcePin;
        TargetPipelineNodeId = targetPipelineNodeId;
        TargetPin = targetPin;
        Kind = kind ?? ((string.Equals(sourcePin, "exec_out", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(targetPin, "exec_in", StringComparison.OrdinalIgnoreCase))
            ? EdgeKind.Exec
            : EdgeKind.Data);
    }
}
