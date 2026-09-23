using Automation.Pipeline.Domain.ValueObjects;
using Automation.Pipeline.Features.Pipelines.Dtos;
using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Pipelines.UpdatePipelineVariables;

[Transactional(typeof(PipelineDbContext))]
public class UpdatePipelineVariablesHandler(
    PipelineDbContext db
)
{
    public async Task<Result<List<PipelineVariableDto>>> HandleAsync(
        UpdatePipelineVariablesCommand command,
        CancellationToken ct
    )
    {
        var pipeline = await db.Pipelines
            .FirstOrDefaultAsync(x => x.Id == command.PipelineId, ct);

        if (pipeline == null)
        {
            return Result.Fail<List<PipelineVariableDto>>($"Pipeline '{command.PipelineId}' not found.");
        }

        var decls = (command.Variables ?? new())
            .Select(v => new PipelineVariableDecl
            {
                Name = v.Name.Trim(),
                Type = v.Type,
                Cardinality = v.Cardinality,
                Description = v.Description,
                StructType = v.StructType
            })
            .ToList();

        pipeline.SetVariables(decls);
        await db.SaveChangesAsync(ct);

        var resultDtos = decls.Select(v => new PipelineVariableDto(
            v.Name,
            v.Type,
            v.Cardinality,
            v.Description,
            v.StructType
        )).ToList();

        return Result.Ok(resultDtos);
    }
}
