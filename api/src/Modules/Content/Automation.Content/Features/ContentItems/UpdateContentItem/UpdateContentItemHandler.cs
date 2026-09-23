using Automation.Content.Domain.Entities;
using Automation.Content.Infrastructure.Persistence;
using Automation.Content.Shared.Dtos;
using Automation.DynamicForms.Contracts;
using Automation.Files.Contracts;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;
using Automation.Content.Constants;

namespace Automation.Content.Features.ContentItems.UpdateContentItem;

[Transactional(typeof(ContentDbContext))]
public class UpdateContentItemHandler(ContentDbContext db, ISchemaApi schemaApi, IAssetApi assetApi)
{
    public async Task<Result<ContentItemDto>> HandleAsync(
        UpdateContentItemCommand request,
        CancellationToken cancellationToken)
    {
        var item = await db.ContentItems
            .Include(x => x.ContentType)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            
        if (item is null) return Result.Fail(new NotFoundError("ContentItem not found"));
        
        item.Update(request.Name);
        await db.SaveChangesAsync(cancellationToken);

        var dataResult = await schemaApi.SaveDataAsync(
            "ContentType", 
            item.ContentTypeId.ToString(), 
            item.Id.ToString(), 
            item.ContentType.Key, 
            request.Values, 
            cancellationToken);

        if (dataResult.IsFailed)
        {
            return dataResult.ToResult<ContentItemDto>();
        }

        
        if (request.ThumbnailAssetId != null)
        {
            await assetApi.VerifyAndLinkAsync(
                request.ThumbnailAssetId.Value,
                nameof(ContentItem),
                ContentAssetSlots.ContentThumbnail,
                item.Id.ToString(),
                request.ThumbnailFileName ?? "Thumbnail",
                0,
                cancellationToken
            );
        }
        else {
            await assetApi.RemoveLinkAsync(
                item.Id.ToString(),
                nameof(ContentItem),
                ContentAssetSlots.ContentThumbnail,
                cancellationToken
            );
        }
        
        return Result.Ok(new ContentItemDto
        {
            Id = item.Id,
            ContentTypeId = item.ContentTypeId,
            ProjectId = item.ProjectId,
            Name = item.Name,
            Values = dataResult.Value.Values
        });
    }
}

