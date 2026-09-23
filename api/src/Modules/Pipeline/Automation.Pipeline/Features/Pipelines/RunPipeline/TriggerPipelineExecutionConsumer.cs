using Automation.Pipeline.Engine;
using Microsoft.Extensions.Logging;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Pipelines.RunPipeline;

[NonTransactional]
public class TriggerPipelineExecutionConsumer(
    Engine.Orchestrator.IPipelineOrchestrator orchestrator,
    ILogger<TriggerPipelineExecutionConsumer> logger
)
{
    public async Task HandleAsync(TriggerPipelineExecutionMessage message, CancellationToken ct)
    {
        logger.LogInformation("Background Triggering Pipeline Execution: {ExecutionId}", message.ExecutionId);
        var result = await orchestrator.ExecuteOrResumeAsync(message.ExecutionId, ct: ct);
        if (result.IsFailed)
        {
            logger.LogError("Background Pipeline Execution {ExecutionId} failed: {Errors}",
                message.ExecutionId,
                string.Join(", ", result.Errors));
        }
    }
}
