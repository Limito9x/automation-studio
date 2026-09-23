using Automation.Files.Contracts;
using Automation.Pipeline.Constants;
using Automation.Pipeline.Domain.ValueObjects;
using Automation.Pipeline.Features.Nodes.CreateCustomNode;
using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Nodes.UpdateCustomNode;

[Transactional(typeof(PipelineDbContext))]
public class UpdateCustomNodeHandler(
    PipelineDbContext db,
    IAssetApi assetApi
)
{
    public async Task<Result<CreateCustomNodeResponseDto>> HandleAsync(
        UpdateCustomNodeCommand command,
        CancellationToken ct
    )
    {
        var node = await db.NodeDefinitions
            .FirstOrDefaultAsync(x => x.Id == command.Id, ct);

        if (node == null)
        {
            return Result.Fail<CreateCustomNodeResponseDto>("Custom node not found.");
        }

        var label = string.IsNullOrWhiteSpace(command.Label) ? command.Name : command.Label.Trim();
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

        node.Update(command.Name, label, executor, sanitizedInputs, sanitizedOutputs);
        await db.SaveChangesAsync(ct);

        // Update script asset link if new AssetId provided
        if (command.AssetId.HasValue && command.AssetId.Value != Guid.Empty)
        {
            // Remove old link
            await assetApi.RemoveLinkAsync(
                ownerEntityId: node.Id.ToString(),
                ownerEntityType: "NodeDefinition",
                slotKey: PipelineAssetSlots.CustomScript,
                ct: ct
            );

            // Create new link
            var fileName = string.IsNullOrWhiteSpace(command.OriginalFileName) ? $"{node.Key}.py" : command.OriginalFileName;
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
                return Result.Fail<CreateCustomNodeResponseDto>($"Updated node but failed to link asset script: {linkResult.Errors.FirstOrDefault()?.Message}");
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
