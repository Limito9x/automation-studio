using Automation.Content.Domain.Entities;
using Automation.Content.Infrastructure.Persistence;
using Automation.Content.Shared.Dtos;
using Automation.DynamicForms.Contracts;
using Automation.DynamicForms.Contracts;
using Automation.SharedKernel.Errors;
using Automation.SharedKernel.Extensions.Strings;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Wolverine.Attributes;

namespace Automation.Content.Features.ContentTypes.CreateContentType;

[Transactional(typeof(ContentDbContext))]
public class CreateContentTypeHandler(ContentDbContext db, ISchemaApi schemaApi)
{
    public async Task<Result<ContentTypeDto>> HandleAsync(
        CreateContentTypeCommand request,
        CancellationToken cancellationToken)
    {
        var key = request.Name.ToSlug();

        var existingType = await db.ContentTypes
            .AnyAsync(c => c.ProjectId == request.ProjectId && c.Key == key, cancellationToken);
            
        if (existingType)
        {
            return Result.Fail(new Error($"ContentType with key '{key}' already exists in this project."));
        }

        var contentType = new ContentType(
            request.ProjectId,
            key,
            request.Name,
            request.DisplayName,
            request.Description,
            request.Icon,
            request.Color,
            request.SortOrder,
            request.DisplayConfig
        );
        
        db.ContentTypes.Add(contentType);
        await db.SaveChangesAsync(cancellationToken);
        
        var emptyFields = JsonDocument.Parse("[]");
        var schemaResult = await schemaApi.UpsertSchemaAsync(
            "ContentType", 
            contentType.Id.ToString(), 
            contentType.Name, 
            emptyFields, 
            cancellationToken);

        if (schemaResult.IsFailed)
        {
            return schemaResult.ToResult<ContentTypeDto>();
        }
        
        return Result.Ok(new ContentTypeDto
        {
            Id = contentType.Id,
            ProjectId = contentType.ProjectId,
            Key = contentType.Key,
            Name = contentType.Name,
            DisplayName = contentType.DisplayName,
            Description = contentType.Description,
            Icon = contentType.Icon,
            Color = contentType.Color,
            SortOrder = contentType.SortOrder,
            DisplayConfig = contentType.DisplayConfig
        });
    }
}

