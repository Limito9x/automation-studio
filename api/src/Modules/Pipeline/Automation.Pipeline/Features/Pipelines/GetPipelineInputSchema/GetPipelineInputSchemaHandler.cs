using Automation.Pipeline.Features.Pipelines.Dtos;
using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Pipelines.GetPipelineInputSchema;

[NonTransactional]
public class GetPipelineInputSchemaHandler(PipelineDbContext db)
{
    public async Task<Result<IReadOnlyList<PipelineInputDto>>> HandleAsync(
        GetPipelineInputSchemaQuery query,
        CancellationToken ct
    )
    {
        var pipeline = await db.Pipelines
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == query.PipelineId, ct);

        if (pipeline == null)
        {
            return Result.Fail<IReadOnlyList<PipelineInputDto>>($"Pipeline '{query.PipelineId}' not found.");
        }

        if (pipeline.TriggerType == Domain.Enums.PipelineTriggerType.OnResourceCreated ||
            pipeline.TriggerType == Domain.Enums.PipelineTriggerType.OnResourceVersionUpdated)
        {
            var eventInputs = new List<PipelineInputDto>
            {
                new PipelineInputDto(
                    Guid.NewGuid(),
                    "Resource",
                    "Resource",
                    Domain.Enums.PinPrimitiveType.EntityRef,
                    Domain.Enums.PinCardinality.Single,
                    true,
                    null,
                    0
                ),
                new PipelineInputDto(
                    Guid.NewGuid(),
                    "Workspace",
                    "Workspace",
                    Domain.Enums.PinPrimitiveType.EntityRef,
                    Domain.Enums.PinCardinality.Single,
                    false,
                    null,
                    1
                )
            };
            return Result.Ok<IReadOnlyList<PipelineInputDto>>(eventInputs);
        }

        var inputs = await db.PipelineInputs
            .AsNoTracking()
            .Where(x => x.PipelineId == query.PipelineId)
            .OrderBy(x => x.Order)
            .Select(i => new PipelineInputDto(
                i.Id,
                i.Key,
                i.Label,
                i.Type,
                i.Cardinality,
                i.IsRequired,
                i.DefaultValue,
                i.Order
            ))
            .ToListAsync(ct);

        return Result.Ok<IReadOnlyList<PipelineInputDto>>(inputs);
    }
}
