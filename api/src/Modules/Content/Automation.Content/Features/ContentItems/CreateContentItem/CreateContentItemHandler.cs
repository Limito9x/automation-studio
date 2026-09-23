using Automation.Content.Constants;
using Automation.Content.Domain.Entities;
using Automation.Content.Infrastructure.Persistence;
using Automation.Content.Shared.Dtos;
using Automation.DynamicForms.Contracts;
using Automation.Files.Contracts;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Content.Features.ContentItems.CreateContentItem;

[Transactional(typeof(ContentDbContext))]
public class CreateContentItemHandler(ContentDbContext db, ISchemaApi schemaApi, IAssetApi assetApi)
{
    public async Task<Result<ContentItemDto>> HandleAsync(
        CreateContentItemCommand request,
        CancellationToken cancellationToken)
    {
        var contentType = await db.ContentTypes
            .FirstOrDefaultAsync(c => c.Key == request.Key && c.ProjectId == request.ProjectId, cancellationToken);

        if (contentType is null)
        {
            return Result.Fail("ContentType not found");
        }

        var item = new ContentItem(
            contentType.Id,
            request.ProjectId,
            request.Name
        );

        db.ContentItems.Add(item);
        await db.SaveChangesAsync(cancellationToken);

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
        
        var dataResult = await schemaApi.SaveDataAsync(
            "ContentType", 
            contentType.Id.ToString(), 
            item.Id.ToString(), 
            contentType.Key, 
            request.Values, 
            cancellationToken);

        if (dataResult.IsFailed)
        {
            return dataResult.ToResult<ContentItemDto>();
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

