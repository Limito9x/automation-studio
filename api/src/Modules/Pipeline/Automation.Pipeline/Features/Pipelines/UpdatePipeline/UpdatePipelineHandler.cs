using Automation.Pipeline.Features.Pipelines.Dtos;
using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Pipelines.UpdatePipeline;

[Transactional(typeof(PipelineDbContext))]
public class UpdatePipelineHandler(PipelineDbContext db)
{
    public async Task<Result<PipelineSummaryDto>> HandleAsync(
        UpdatePipelineCommand command,
        CancellationToken ct
    )
    {
        var trimmedName = command.Name.Trim();
        var pipeline = await db.Pipelines
            .Include(x => x.Nodes)
            .Include(x => x.Edges)
            .FirstOrDefaultAsync(x => x.Id == command.Id, ct);

        if (pipeline == null)
        {
            return Result.Fail<PipelineSummaryDto>($"Pipeline '{command.Id}' not found.");
        }

        var exists = await db.Pipelines.AnyAsync(
            x => x.ProjectId == pipeline.ProjectId &&
                 x.Id != pipeline.Id &&
                 (x.Name == trimmedName || x.Name.ToLower() == trimmedName.ToLower()),
            ct
        );

        if (exists)
        {
            return Result.Fail<PipelineSummaryDto>($"A pipeline with name '{trimmedName}' already exists in this project.");
        }

        pipeline.UpdateName(trimmedName);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Pipelines_ProjectId_Name") == true || ex.Message.Contains("IX_Pipelines_ProjectId_Name"))
        {
            return Result.Fail<PipelineSummaryDto>($"A pipeline with name '{trimmedName}' already exists in this project.");
        }

        var dto = new PipelineSummaryDto(
            pipeline.Id,
            pipeline.ProjectId,
            pipeline.Name,
            pipeline.TriggerType,
            pipeline.TriggerWorkspaceId,
            pipeline.Nodes.Count,
            pipeline.Edges.Count,
            pipeline.CreatedAt
        );

        return Result.Ok(dto);
    }
}
