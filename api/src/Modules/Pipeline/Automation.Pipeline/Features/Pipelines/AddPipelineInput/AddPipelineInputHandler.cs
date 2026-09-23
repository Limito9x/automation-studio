using Automation.Pipeline.Domain.Entities;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Features.Pipelines.Dtos;
using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Pipelines.AddPipelineInput;

[Transactional(typeof(PipelineDbContext))]
public class AddPipelineInputHandler(PipelineDbContext db)
{
    public async Task<Result<PipelineInputDto>> HandleAsync(
        AddPipelineInputCommand command,
        CancellationToken ct
    )
    {
        var pipelineExists = await db.Pipelines.AsNoTracking().AnyAsync(x => x.Id == command.PipelineId, ct);
        if (!pipelineExists)
        {
            return Result.Fail<PipelineInputDto>($"Pipeline '{command.PipelineId}' not found.");
        }

        var key = command.Key.Trim();
        if (string.IsNullOrWhiteSpace(key))
        {
            return Result.Fail<PipelineInputDto>("Input key cannot be empty.");
        }

        var keyExists = await db.PipelineInputs.AsNoTracking().AnyAsync(
            x => x.PipelineId == command.PipelineId && x.Key.ToLower() == key.ToLower(),
            ct
        );
        if (keyExists)
        {
            return Result.Fail<PipelineInputDto>($"Input key '{key}' already exists in this pipeline.");
        }

        if (!Enum.TryParse<PinPrimitiveType>(command.Type, true, out var pinType))
        {
            pinType = PinPrimitiveType.String;
        }

        if (!Enum.TryParse<PinCardinality>(command.Cardinality, true, out var cardinality))
        {
            cardinality = PinCardinality.Single;
        }

        var label = string.IsNullOrWhiteSpace(command.Label) ? key : command.Label.Trim();
        var order = command.Order;
        if (order == 0)
        {
            var maxOrder = await db.PipelineInputs
                .Where(x => x.PipelineId == command.PipelineId)
                .Select(x => (int?)x.Order)
                .MaxAsync(ct) ?? 0;
            order = maxOrder + 1;
        }

        var input = new PipelineInput(
            command.PipelineId,
            key,
            label,
            pinType,
            cardinality,
            command.IsRequired,
            command.DefaultValue,
            order
        );

        await db.PipelineInputs.AddAsync(input, ct);
        await db.SaveChangesAsync(ct);

        var dto = new PipelineInputDto(
            input.Id,
            input.Key,
            input.Label,
            input.Type,
            input.Cardinality,
            input.IsRequired,
            input.DefaultValue,
            input.Order
        );

        return Result.Ok(dto);
    }
}
