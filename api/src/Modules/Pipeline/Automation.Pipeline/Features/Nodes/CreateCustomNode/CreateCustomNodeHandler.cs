using Automation.Files.Contracts;
using Automation.Pipeline.Constants;
using Automation.Pipeline.Domain.Entities;
using Automation.Pipeline.Domain.ValueObjects;
using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Nodes.CreateCustomNode;

[Transactional(typeof(PipelineDbContext))]
public class CreateCustomNodeHandler(
    PipelineDbContext db,
    IAssetApi assetApi
)
{
    public async Task<Result<CreateCustomNodeResponseDto>> HandleAsync(
        CreateCustomNodeCommand command,
        CancellationToken ct
    )
    {
        var nameTrimmed = command.Name.Trim();
        var key = nameTrimmed.Replace(" ", "-").ToLowerInvariant();
        var label = string.IsNullOrWhiteSpace(command.Label) ? nameTrimmed : command.Label.Trim();
        var executor = string.IsNullOrWhiteSpace(command.Executor) ? "blender" : command.Executor.Trim().ToLowerInvariant();

        var sanitizedInputs = (command.Inputs ?? []).Select(p => new PinDefinition
        {
            Id = p.Id,
            Label = p.Label,
            PrimitiveType = p.PrimitiveType,
            Cardinality = p.Cardinality,
            IsRequired = p.IsRequired,
            DefaultValue = p.DefaultValue?.ToString(),
            Metadata = p.Metadata
        }).ToList();

        var sanitizedOutputs = (command.Outputs ?? []).Select(p => new PinDefinition
        {
            Id = p.Id,
            Label = p.Label,
            PrimitiveType = p.PrimitiveType,
            Cardinality = p.Cardinality,
            IsRequired = p.IsRequired,
            DefaultValue = p.DefaultValue?.ToString(),
            Metadata = p.Metadata
        }).ToList();

        // Intelligent Upsert: Find existing record (active or soft-deleted) by ProjectId + Key OR Name + Executor
        var existing = await db.NodeDefinitions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.ProjectId == command.ProjectId &&
                (x.Key == key || (x.Name.ToLower() == nameTrimmed.ToLower() && x.Executor.ToLower() == executor)), ct);

        NodeDefinition node;
        if (existing != null)
        {
            existing.Update(nameTrimmed, label, executor, sanitizedInputs, sanitizedOutputs);
            if (existing.IsDeleted)
            {
                existing.Restore();
            }
            node = existing;
        }
        else
        {
            node = new NodeDefinition(
                command.ProjectId,
                nameTrimmed,
                key,
                label,
                executor,
                sanitizedInputs,
                sanitizedOutputs
            );
            db.NodeDefinitions.Add(node);
        }

        await db.SaveChangesAsync(ct);

        // Link script file via IAssetApi if AssetId provided
        if (command.AssetId.HasValue && command.AssetId.Value != Guid.Empty)
        {
            var fileName = string.IsNullOrWhiteSpace(command.OriginalFileName) ? $"{key}.py" : command.OriginalFileName;
            var linkResult = await assetApi.VerifyAndLinkAsync(
                command.AssetId.Value,
                ownerEntityType: "NodeDefinition",
                slotKey: PipelineAssetSlots.CustomScript,
                ownerEntityId: node.Id.ToString(),
                originalName: fileName,
                sortOrder: 0,
                ct: ct
            );

            if (linkResult.IsFailed)
            {
                return Result.Fail<CreateCustomNodeResponseDto>($"Created node definition but failed to link asset script: {linkResult.Errors.FirstOrDefault()?.Message}");
            }
        }

        return Result.Ok(new CreateCustomNodeResponseDto(
            node.Id,
            node.ProjectId,
            node.Name,
            node.Key,
            node.Label,
            node.Executor,
            node.Inputs,
            node.Outputs,
            node.CreatedAt
        ));
    }
}
