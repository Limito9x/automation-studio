using Automation.Content.Infrastructure.Persistence;
using Automation.DynamicForms.Contracts;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Content.Features.ContentTypes.UpdateContentTypeSchema;



[Transactional(typeof(ContentDbContext))]
public class UpdateContentTypeSchemaHandler(ContentDbContext db, ISchemaApi schemaApi)
{
    public async Task<Result> HandleAsync(
        UpdateContentTypeSchemaCommand request,
        CancellationToken cancellationToken)
    {
        var item = await db.ContentTypes.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (item is null) return Result.Fail(new NotFoundError("ContentType not found"));
        
        var schemaResult = await schemaApi.UpsertSchemaAsync(
            "ContentType", 
            item.Id.ToString(), 
            item.Name, 
            request.FieldsConfig, 
            cancellationToken);

        return schemaResult;
    }
}

