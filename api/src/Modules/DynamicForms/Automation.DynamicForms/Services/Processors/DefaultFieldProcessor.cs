using System.Text.Json;
using System.Text.Json.Nodes;

namespace Automation.DynamicForms.Services.Processors;

public class DefaultFieldProcessor : IFieldTypeProcessor
{
    public const string DefaultType = "default";
    public string FieldType => DefaultType;

    public Result Validate(FieldDefinitionContext field, JsonElement value, FieldValidationContext ctx)
    {
        bool isNullOrEmpty = value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined ||
                             (value.ValueKind == JsonValueKind.String && string.IsNullOrWhiteSpace(value.GetString()));

        if (field.IsRequired && isNullOrEmpty)
        {
            return Result.Fail(new Error(field.RequiredMessage).WithMetadata("Field", field.Name));
        }

        return Result.Ok();
    }

    public JsonNode? Normalize(FieldDefinitionContext field, JsonElement? rawValue)
    {
        if (rawValue.HasValue && rawValue.Value.ValueKind != JsonValueKind.Null && rawValue.Value.ValueKind != JsonValueKind.Undefined)
        {
            return JsonNode.Parse(rawValue.Value.GetRawText());
        }

        return null;
    }

    public Task<Result> BeforeSaveAsync(FieldDefinitionContext field, JsonElement value, FieldSaveContext ctx, CancellationToken ct = default)
    {
        return Task.FromResult(Result.Ok());
    }

    public Task<JsonNode?> ResolveAsync(FieldDefinitionContext field, JsonElement value, FieldResolveContext ctx, CancellationToken ct = default)
    {
        if (value.ValueKind is not (JsonValueKind.Null or JsonValueKind.Undefined))
        {
            return Task.FromResult<JsonNode?>(JsonNode.Parse(value.GetRawText()));
        }

        return Task.FromResult<JsonNode?>(null);
    }
}
