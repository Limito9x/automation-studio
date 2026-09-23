using Automation.Pipeline.Domain.Entities;
using Automation.Pipeline.Engine.Models;

namespace Automation.Pipeline.Engine;

public interface IPipelineExecutionEngine
{
    Task<Result<PipelineExecution>> ExecuteOrResumeAsync(
        Guid executionId,
        Dictionary<string, object?>? runtimeInputs = null,
        CancellationToken ct = default
    );
}
