using System.Text.Json;
using System.Text.Json.Nodes;
using Automation.DynamicForms.Constants;
using Automation.DynamicForms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Automation.DynamicForms.Services.Processors;

public class StructFieldProcessor(DynamicFormsDbContext? db = null) : IFieldTypeProcessor
{
    public string FieldType => SchemaType.Struct;

    public Result Validate(FieldDefinitionContext field, JsonElement value, FieldValidationContext ctx)
    {
        bool isNullOrEmpty = value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined ||
                             (value.ValueKind == JsonValueKind.Array && value.GetArrayLength() == 0) ||
                             (value.ValueKind == JsonValueKind.Object && !value.EnumerateObject().Any());

        if (field.IsRequired && isNullOrEmpty)
        {
            return Result.Fail(new Error(field.RequiredMessage).WithMetadata("Field", field.Name));
        }

        if (isNullOrEmpty)
        {
            return Result.Ok();
        }

        if (field.StructId == null)
        {
            return Result.Fail(new Error($"Struct field '{field.Name}' has no structId configured.").WithMetadata("Field", field.Name));
        }

        var childFields = GetChildSchemaFields(field.StructId.Value, ctx.PreloadedSchemas);
        if (childFields == null)
        {
            return Result.Fail(new Error($"Target Struct '{field.StructId.Value}' schema not found for field '{field.Name}'.").WithMetadata("Field", field.Name));
        }

        if (ctx.Engine == null)
        {
            return Result.Ok();
        }

        var errors = new List<IError>();

        switch (field.Cardinality.ToLowerInvariant())
        {
            case "array":
                if (value.ValueKind == JsonValueKind.Array)
                {
                    int index = 0;
                    foreach (var item in value.EnumerateArray())
                    {
                        var itemDoc = JsonDocument.Parse(item.GetRawText());
                        var childResult = ctx.Engine.ValidateValues(childFields, itemDoc);
                        if (childResult.IsFailed)
                        {
                            foreach (var err in childResult.Errors)
                            {
                                errors.Add(new Error($"[{index}].{err.Message}"));
                            }
                        }
                        index++;
                    }
                }
                else
                {
                    errors.Add(new Error($"Field '{field.Name}' expected an array of structs."));
                }
                break;

            case "map":
                if (value.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in value.EnumerateObject())
                    {
                        var itemDoc = JsonDocument.Parse(prop.Value.GetRawText());
                        var childResult = ctx.Engine.ValidateValues(childFields, itemDoc);
                        if (childResult.IsFailed)
                        {
                            foreach (var err in childResult.Errors)
                            {
                                errors.Add(new Error($"[{prop.Name}].{err.Message}"));
                            }
                        }
                    }
                }
                else
                {
                    errors.Add(new Error($"Field '{field.Name}' expected a map of structs."));
                }
                break;

            case "single":
            default:
                if (value.ValueKind == JsonValueKind.Object)
                {
                    var itemDoc = JsonDocument.Parse(value.GetRawText());
                    var childResult = ctx.Engine.ValidateValues(childFields, itemDoc);
                    if (childResult.IsFailed)
                    {
                        errors.AddRange(childResult.Errors);
                    }
                }
                else
                {
                    errors.Add(new Error($"Field '{field.Name}' expected a struct object."));
                }
                break;
        }

        return errors.Count > 0 ? Result.Fail(errors) : Result.Ok();
    }

    public JsonNode? Normalize(FieldDefinitionContext field, JsonElement? rawValue)
    {
        if (!rawValue.HasValue || rawValue.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        return JsonNode.Parse(rawValue.Value.GetRawText());
    }

    public async Task<Result> BeforeSaveAsync(FieldDefinitionContext field, JsonElement value, FieldSaveContext ctx, CancellationToken ct = default)
    {
        if (value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined || field.StructId == null || ctx.Engine == null)
        {
            return Result.Ok();
        }

        var childFields = await GetChildSchemaFieldsAsync(field.StructId.Value, ctx.PreloadedSchemas, ct);
        if (childFields == null)
        {
            return Result.Ok();
        }

        switch (field.Cardinality.ToLowerInvariant())
        {
            case "array":
                if (value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in value.EnumerateArray())
                    {
                        using var itemDoc = JsonDocument.Parse(item.GetRawText());
                        var linkRes = await ctx.Engine.LinkFileFieldsAsync(ctx.SchemaDataId, childFields, itemDoc, ct, ctx.PreloadedSchemas);
                        if (linkRes.IsFailed) return linkRes;
                    }
                }
                break;

            case "map":
                if (value.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in value.EnumerateObject())
                    {
                        using var itemDoc = JsonDocument.Parse(prop.Value.GetRawText());
                        var linkRes = await ctx.Engine.LinkFileFieldsAsync(ctx.SchemaDataId, childFields, itemDoc, ct, ctx.PreloadedSchemas);
                        if (linkRes.IsFailed) return linkRes;
                    }
                }
                break;

            case "single":
            default:
                if (value.ValueKind == JsonValueKind.Object)
                {
                    using var itemDoc = JsonDocument.Parse(value.GetRawText());
                    return await ctx.Engine.LinkFileFieldsAsync(ctx.SchemaDataId, childFields, itemDoc, ct, ctx.PreloadedSchemas);
                }
                break;
        }

        return Result.Ok();
    }

    public async Task<JsonNode?> ResolveAsync(FieldDefinitionContext field, JsonElement value, FieldResolveContext ctx, CancellationToken ct = default)
    {
        if (value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined || field.StructId == null || ctx.Engine == null)
        {
            return value.ValueKind is not (JsonValueKind.Null or JsonValueKind.Undefined)
                ? JsonNode.Parse(value.GetRawText())
                : null;
        }

        var childFields = await GetChildSchemaFieldsAsync(field.StructId.Value, ctx.PreloadedSchemas, ct);
        if (childFields == null)
        {
            return JsonNode.Parse(value.GetRawText());
        }

        switch (field.Cardinality.ToLowerInvariant())
        {
            case "array":
                if (value.ValueKind == JsonValueKind.Array)
                {
                    var arrayNode = new JsonArray();
                    foreach (var item in value.EnumerateArray())
                    {
                        using var itemDoc = JsonDocument.Parse(item.GetRawText());
                        var resolvedItem = await ctx.Engine.ResolveDataAsync(ctx.SchemaDataId, childFields, itemDoc, ct, ctx.PreloadedSchemas);
                        if (resolvedItem.IsSuccess)
                        {
                            arrayNode.Add(JsonNode.Parse(resolvedItem.Value.RootElement.GetRawText()));
                        }
                        else
                        {
                            arrayNode.Add(JsonNode.Parse(item.GetRawText()));
                        }
                    }
                    return arrayNode;
                }
                break;

            case "map":
                if (value.ValueKind == JsonValueKind.Object)
                {
                    var mapNode = new JsonObject();
                    foreach (var prop in value.EnumerateObject())
                    {
                        using var itemDoc = JsonDocument.Parse(prop.Value.GetRawText());
                        var resolvedItem = await ctx.Engine.ResolveDataAsync(ctx.SchemaDataId, childFields, itemDoc, ct, ctx.PreloadedSchemas);
                        if (resolvedItem.IsSuccess)
                        {
                            mapNode[prop.Name] = JsonNode.Parse(resolvedItem.Value.RootElement.GetRawText());
                        }
                        else
                        {
                            mapNode[prop.Name] = JsonNode.Parse(prop.Value.GetRawText());
                        }
                    }
                    return mapNode;
                }
                break;

            case "single":
            default:
                if (value.ValueKind == JsonValueKind.Object)
                {
                    using var itemDoc = JsonDocument.Parse(value.GetRawText());
                    var resolvedItem = await ctx.Engine.ResolveDataAsync(ctx.SchemaDataId, childFields, itemDoc, ct, ctx.PreloadedSchemas);
                    if (resolvedItem.IsSuccess)
                    {
                        return JsonNode.Parse(resolvedItem.Value.RootElement.GetRawText());
                    }
                }
                break;
        }

        return JsonNode.Parse(value.GetRawText());
    }

    private JsonDocument? GetChildSchemaFields(Guid structId, Dictionary<Guid, JsonDocument> preloaded)
    {
        if (preloaded.TryGetValue(structId, out var cachedDoc))
        {
            return cachedDoc;
        }

        if (db == null) return null;

        var schema = db.SchemaDefinitions
            .Include(s => s.Versions)
            .AsNoTracking()
            .FirstOrDefault(s => s.Id == structId);

        var activeVersion = schema?.Versions.FirstOrDefault(v => v.IsActive);
        if (activeVersion != null)
        {
            preloaded[structId] = activeVersion.Fields;
            return activeVersion.Fields;
        }

        return null;
    }

    private async Task<JsonDocument?> GetChildSchemaFieldsAsync(Guid structId, Dictionary<Guid, JsonDocument> preloaded, CancellationToken ct)
    {
        if (preloaded.TryGetValue(structId, out var cachedDoc))
        {
            return cachedDoc;
        }

        if (db == null) return null;

        var schema = await db.SchemaDefinitions
            .Include(s => s.Versions)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == structId, ct);

        var activeVersion = schema?.Versions.FirstOrDefault(v => v.IsActive);
        if (activeVersion != null)
        {
            preloaded[structId] = activeVersion.Fields;
            return activeVersion.Fields;
        }

        return null;
    }
}
