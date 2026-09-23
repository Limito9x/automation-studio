using Automation.DynamicForms.Constants;
using Automation.DynamicForms.Contracts;
using Automation.DynamicForms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.DynamicForms.Features.Structs.DeleteStruct;

[Transactional(typeof(DynamicFormsDbContext))]
public class DeleteStructHandler(DynamicFormsDbContext db, ISchemaApi schemaApi)
{
    public async Task<Result> HandleAsync(
        DeleteStructCommand command,
        CancellationToken ct)
    {
        var ownerId = command.ProjectId.ToString();

        var schema = await db.SchemaDefinitions
            .FirstOrDefaultAsync(s => s.Id == command.Id &&
                                      s.OwnerType == DynamicFormsOwnerType.ProjectStruct &&
                                      s.OwnerId == ownerId, ct);

        if (schema == null)
        {
            return Result.Fail(new NotFoundError($"Struct with id '{command.Id}' not found in this project."));
        }

        // Referential Integrity Check: Verify no active schema references this struct
        var referencingResult = await schemaApi.GetReferencingSchemasAsync(command.Id, ct);
        if (referencingResult.IsSuccess && referencingResult.Value.Count > 0)
        {
            var usageDescriptions = string.Join(", ", referencingResult.Value.Select(u => $"'{u.Name}' ({u.OwnerType})"));
            return Result.Fail(new Error($"Cannot delete Struct '{schema.Name}' because it is referenced by: {usageDescriptions}."));
        }

        db.SchemaDefinitions.Remove(schema);
        await db.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
