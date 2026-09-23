using System.Text.Json;
using Automation.Pipeline.Domain.Enums;

namespace Automation.Pipeline.Domain.Entities;

public class NodeExecution : AuditableEntity
{
    public Guid PipelineExecutionId { get; set; }
    public PipelineExecution PipelineExecution { get; set; } = null!;
    public Guid PipelineNodeId { get; set; }
    public PipelineNode PipelineNode { get; set; } = null!;
    public ExecutionStatus Status { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public JsonDocument? Output { get; set; }
    public JsonDocument? Log { get; set; }
    public JsonDocument? Progress { get; set; }

    public NodeExecution() { }

    public NodeExecution(
        Guid pipelineExecutionId,
        Guid pipelineNodeId,
        JsonDocument? progress = null,
        ExecutionStatus status = ExecutionStatus.Pending
    )
    {
        PipelineExecutionId = pipelineExecutionId;
        PipelineNodeId = pipelineNodeId;
        Status = status;
        Progress = progress;
    }

    public void MarkRunning()
    {
        Status = ExecutionStatus.Running;
        StartedAt = DateTimeOffset.UtcNow;
        FinishedAt = null;
        ErrorMessage = null;
    }

    public void MarkSucceeded(JsonDocument output, JsonDocument? log = null)
    {
        Status = ExecutionStatus.Succeeded;
        FinishedAt = DateTimeOffset.UtcNow;
        Output = output;
        Log = log;
    }

    public void MarkFailed(string error, JsonDocument? log = null)
    {
        Status = ExecutionStatus.Failed;
        FinishedAt = DateTimeOffset.UtcNow;
        ErrorMessage = error;
        Log = log;
    }

    public void UpdateProgress(JsonDocument progress)
    {
        Progress = progress;
    }
}
