using Automation.Pipeline.Domain.Entities;

namespace Automation.Pipeline.Engine.DataResolver;

public interface IPipelineGraphProvider
{
    Task<PipelineExecution?> GetExecutionByIdAsync(Guid executionId, CancellationToken ct = default);
    Task<Domain.Entities.Pipeline?> GetPipelineByExecutionIdAsync(Guid executionId, CancellationToken ct = default);
    Task<Domain.Entities.Pipeline?> GetPipelineByIdAsync(Guid pipelineId, CancellationToken ct = default);
}
