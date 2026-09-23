using Automation.Pipeline.Engine;
using Automation.Pipeline.Features.Pipelines.Dtos;
using Automation.Pipeline.Infrastructure.Persistence;
using Automation.Pipeline.Tools;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Pipelines.ValidatePipeline;

[NonTransactional]
public class ValidatePipelineHandler(
    PipelineDbContext db,
    Engine.ExecPlanner.IExecPlanner planner,
    IToolRegistry toolRegistry
)
{
    public async Task<Result<ValidatePipelineResponse>> HandleAsync(
        ValidatePipelineQuery query,
        CancellationToken ct
    )
    {
        var pipeline = await db.Pipelines
            .Include(p => p.Nodes)
            .Include(p => p.Edges)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == query.PipelineId, ct);

        if (pipeline == null)
        {
            return Result.Fail<ValidatePipelineResponse>($"Pipeline '{query.PipelineId}' not found.");
        }

        var customDefs = await db.NodeDefinitions
            .AsNoTracking()
            .Where(x => x.ProjectId == pipeline.ProjectId)
            .ToListAsync(ct);

        var result = planner.BuildExecPlan(
            pipeline,
            customDefs,
            toolRegistry,
            query.RuntimeInputs
        );

        return Result.Ok(new ValidatePipelineResponse(
            result.IsValid,
            result.CycleNodeIds,
            result.UnresolvedPins
        ));
    }
}
