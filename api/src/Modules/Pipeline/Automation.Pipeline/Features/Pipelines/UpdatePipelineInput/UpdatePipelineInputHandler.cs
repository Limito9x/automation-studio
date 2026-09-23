using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Features.Pipelines.Dtos;
using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Pipelines.UpdatePipelineInput;

[Transactional(typeof(PipelineDbContext))]
public class UpdatePipelineInputHandler(PipelineDbContext db)
{
    public async Task<Result<PipelineInputDto>> HandleAsync(
        UpdatePipelineInputCommand command,
        CancellationToken ct
    )
    {
        var input = await db.PipelineInputs
            .FirstOrDefaultAsync(x => x.Id == command.InputId && x.PipelineId == command.PipelineId, ct);

        if (input == null)
        {
            return Result.Fail<PipelineInputDto>($"Pipeline input '{command.InputId}' not found.");
        }

        var key = input.Key;
        if (!string.IsNullOrWhiteSpace(command.Key))
        {
            key = command.Key.Trim();
            if (!string.Equals(key, input.Key, StringComparison.OrdinalIgnoreCase))
            {
                var keyExists = await db.PipelineInputs.AsNoTracking().AnyAsync(
                    x => x.PipelineId == command.PipelineId && x.Id != command.InputId && x.Key.ToLower() == key.ToLower(),
                    ct
                );
                if (keyExists)
                {
                    return Result.Fail<PipelineInputDto>($"Input key '{key}' already exists in this pipeline.");
                }
            }
        }

        var label = command.Label != null ? command.Label.Trim() : input.Label;
        var type = input.Type;
        if (!string.IsNullOrWhiteSpace(command.Type) && Enum.TryParse<PinPrimitiveType>(command.Type, true, out var parsedType))
        {
            type = parsedType;
        }

        var cardinality = input.Cardinality;
        if (!string.IsNullOrWhiteSpace(command.Cardinality) && Enum.TryParse<PinCardinality>(command.Cardinality, true, out var parsedCard))
        {
            cardinality = parsedCard;
        }

        var isRequired = command.IsRequired ?? input.IsRequired;
        var defaultValue = command.DefaultValue ?? input.DefaultValue;
        var order = command.Order ?? input.Order;

        input.Update(
            key,
            label,
            type,
            cardinality,
            isRequired,
            defaultValue,
            order
        );

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
