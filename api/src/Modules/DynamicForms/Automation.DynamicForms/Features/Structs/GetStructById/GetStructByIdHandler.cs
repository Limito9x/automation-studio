using Automation.DynamicForms.Constants;
using Automation.DynamicForms.Contracts;
using Automation.DynamicForms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.DynamicForms.Features.Structs.GetStructById;

[NonTransactional]
public class GetStructByIdHandler(DynamicFormsDbContext db, ISchemaApi schemaApi)
{
    public async Task<Result<StructDetailDto>> HandleAsync(
        GetStructByIdQuery query,
        CancellationToken ct)
    {
        var schemaResult = await schemaApi.GetActiveVersionWithDependenciesAsync(query.Id, ct);
        if (schemaResult.IsFailed)
            return schemaResult.ToResult<StructDetailDto>();

        var schema = schemaResult.Value;
        if (schema.OwnerType != DynamicFormsOwnerType.ProjectStruct || schema.OwnerId != query.ProjectId.ToString())
        {
            return Result.Fail(new NotFoundError($"Struct with id '{query.Id}' not found in this project"));
        }

        var schemaEntity = await db.SchemaDefinitions.AsNoTracking().FirstOrDefaultAsync(s => s.Id == query.Id, ct);

        return Result.Ok(new StructDetailDto
        {
            Id = schema.SchemaDefinitionId,
            ProjectId = query.ProjectId,
            Name = schema.Name,
            ActiveVersion = schema.ActiveVersion,
            Dependencies = schema.Dependencies,
            CreatedAt = schemaEntity?.CreatedAt ?? DateTimeOffset.UtcNow,
            UpdatedAt = schemaEntity?.UpdatedAt
        });
    }
}
