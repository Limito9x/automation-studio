using System.Text.Json;
using Automation.DynamicForms.Constants;
using Automation.DynamicForms.Contracts;
using Automation.DynamicForms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.DynamicForms.Features.Structs.GetProjectStructs;

[NonTransactional]
public class GetProjectStructsHandler(DynamicFormsDbContext db)
{
    public async Task<Result<IReadOnlyList<StructSummaryDto>>> HandleAsync(
        GetProjectStructsQuery query,
        CancellationToken ct)
    {
        var ownerId = query.ProjectId.ToString();

        var queryable = db.SchemaDefinitions
            .Include(s => s.Versions.Where(v => v.IsActive))
            .AsNoTracking()
            .Where(s => s.OwnerType == DynamicFormsOwnerType.ProjectStruct && s.OwnerId == ownerId);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            queryable = queryable.Where(s => s.Name.ToLower().Contains(search));
        }

        var schemas = await queryable
            .OrderBy(s => s.Name)
            .ToListAsync(ct);

        var result = new List<StructSummaryDto>();

        foreach (var schema in schemas)
        {
            var activeVersion = schema.Versions.FirstOrDefault(v => v.IsActive);
            var fieldCount = 0;
            var depIds = new List<Guid>();

            if (activeVersion != null)
            {
                depIds = activeVersion.DependencySchemaIds;
                if (activeVersion.Fields != null && activeVersion.Fields.RootElement.ValueKind == JsonValueKind.Array)
                {
                    fieldCount = activeVersion.Fields.RootElement.GetArrayLength();
                }
            }

            result.Add(new StructSummaryDto
            {
                Id = schema.Id,
                ProjectId = query.ProjectId,
                Name = schema.Name,
                ActiveVersion = activeVersion?.Version ?? 0,
                FieldCount = fieldCount,
                DependencySchemaIds = depIds,
                CreatedAt = schema.CreatedAt,
                UpdatedAt = schema.UpdatedAt
            });
        }

        return Result.Ok<IReadOnlyList<StructSummaryDto>>(result);
    }
}
