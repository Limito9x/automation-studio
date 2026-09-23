using Automation.Content.Infrastructure.Persistence;
using Automation.Content.Shared.Dtos;
using Automation.SharedKernel.Errors;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Content.Features.ContentTypes.UpdateContentType;

[Transactional(typeof(ContentDbContext))]
public class UpdateContentTypeHandler(ContentDbContext db)
{
    public async Task<Result<ContentTypeDto>> HandleAsync(
        UpdateContentTypeCommand request,
        CancellationToken cancellationToken)
    {
        var item = await db.ContentTypes.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (item is null) return Result.Fail(new NotFoundError("ContentType not found"));
        
        item.Update(
            request.Name,
            request.DisplayName,
            request.Description,
            request.Icon,
            request.Color,
            request.SortOrder,
            request.DisplayConfig
        );
        
        await db.SaveChangesAsync(cancellationToken);
        
        return Result.Ok(new ContentTypeDto
        {
            Id = item.Id,
            ProjectId = item.ProjectId,
            Key = item.Key,
            Name = item.Name,
            DisplayName = item.DisplayName,
            Description = item.Description,
            Icon = item.Icon,
            Color = item.Color,
            SortOrder = item.SortOrder,
            DisplayConfig = item.DisplayConfig
        });
    }
}

