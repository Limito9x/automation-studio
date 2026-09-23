using Automation.Content.Constants;
using Automation.Content.Infrastructure.Persistence;
using Automation.Content.Shared.Dtos;
using Automation.DynamicForms.Contracts;
using Automation.Files.Contracts;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Content.Features.ContentItems.GetContentItemById;

[NonTransactional]
public class GetContentItemByIdHandler(ContentDbContext db, ISchemaApi schemaApi, IAssetApi assetApi)
{
    public async Task<Result<ContentItemDto>> HandleAsync(
        GetContentItemByIdQuery query,
        CancellationToken ct)
    {
        var item = await db.ContentItems
            .Include(x => x.ContentType)
            .FirstOrDefaultAsync(x => x.Id == query.Id, ct);
        
        if (item is null) return Result.Fail(new NotFoundError("ContentItem not found"));
        
        var dataResult = await schemaApi.GetDataAsync(item.Id.ToString(), item.ContentType!.Key, ct);

        Guid? thumbnailAssetId = null;
        string? thumbnailUrl = null;

        var assetResult = await assetApi.GetFilesAsync(item.Id.ToString(), nameof(Domain.Entities.ContentItem), ContentAssetSlots.ContentThumbnail, ct);
        if (assetResult.IsSuccess && assetResult.Value.FirstOrDefault() is { } asset)
        {
            thumbnailAssetId = asset.AssetId;
            thumbnailUrl = asset.PublicUrl;
        }

        return Result.Ok(new ContentItemDto
        {
            Id = item.Id,
            ContentTypeId = item.ContentTypeId,
            ProjectId = item.ProjectId,
            Name = item.Name,
            ResolvedData = dataResult.IsSuccess ? dataResult.Value.ResolvedData : null,
            Values = dataResult.IsSuccess ? dataResult.Value.Values : null,
            ThumbnailAssetId = thumbnailAssetId,
            ThumbnailUrl = thumbnailUrl,
        });
    }
}

