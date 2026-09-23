using Automation.Content.Domain.Entities;
using Automation.Content.Infrastructure.Persistence;
using Automation.Content.Shared.Dtos;
using Automation.DynamicForms.Contracts;
using Gridify;
using Wolverine.Attributes;

namespace Automation.Content.Features.ContentTypes.GetContentTypes;


[NonTransactional]
public class GetContentTypesHandler(ContentDbContext db, ISchemaApi schemaApi)
{
    public async Task<Result<PagedResult<ContentTypeDto>>> HandleAsync(
        GetContentTypesQuery query,
        CancellationToken ct)
    {
        var mapper = new GridifyMapper<ContentType>()
            .GenerateMappings();

        var queryable = db.Set<ContentType>().AsQueryable();
        
        if (query.ProjectId != Guid.Empty)
        {
            queryable = queryable.Where(x => x.ProjectId == query.ProjectId);
        }

        var result = await queryable
            .ToPagedResultAsync<ContentType, ContentTypeDto>(query, mapper, ct);

        if (result.IsSuccess)
        {
            // Fetch schema for each content type (MVP N+1 approach)
            foreach (var item in result.Value.Items)
            {
                var schemaResult = await schemaApi.GetActiveVersionAsync("ContentType", item.Id.ToString(), ct);
                if (schemaResult.IsSuccess)
                {
                    item.FieldsConfig = schemaResult.Value.Fields;
                }
            }
        }
            
        return result;
    }
}

