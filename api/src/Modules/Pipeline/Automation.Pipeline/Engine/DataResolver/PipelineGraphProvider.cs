using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Automation.Pipeline.Engine.DataResolver;

public class PipelineGraphProvider(PipelineDbContext db) : IPipelineGraphProvider
{
    public async Task<Domain.Entities.PipelineExecution?> GetExecutionByIdAsync(Guid executionId, CancellationToken ct = default)
    {
        return await db.PipelineExecutions
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == executionId, ct);
    }

    public async Task<Domain.Entities.Pipeline?> GetPipelineByExecutionIdAsync(Guid executionId, CancellationToken ct = default)
    {
        var execution = await GetExecutionByIdAsync(executionId, ct);

        if (execution == null) return null;

        return await GetPipelineByIdAsync(execution.PipelineId, ct);
    }

    public async Task<Domain.Entities.Pipeline?> GetPipelineByIdAsync(Guid pipelineId, CancellationToken ct = default)
    {
        return await db.Pipelines
            .Include(p => p.Nodes)
            .Include(p => p.Edges)
            .Include(p => p.Inputs)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == pipelineId, ct);
    }
}
