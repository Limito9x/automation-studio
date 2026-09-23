using System.Text.Json;
using System.Text.Json.Nodes;
using Automation.DynamicForms.Services.Processors;

namespace Automation.DynamicForms.Services;

public class DynamicFormEngine : IDynamicFormEngine
{
    private readonly Dictionary<string, IFieldTypeProcessor> _processors;
    private readonly DefaultFieldProcessor _defaultProcessor = new();

    public DynamicFormEngine(IEnumerable<IFieldTypeProcessor> processors)
    {
        _processors = processors.ToDictionary(p => p.FieldType, StringComparer.OrdinalIgnoreCase);
    }

    public Result ValidateValues(
        JsonDocument schemaFields,
        JsonDocument values,
        IDictionary<Guid, JsonDocument>? preloadedSchemas = null)
    {
        var fields = ParseFields(schemaFields);
        var valueObj = values.RootElement.ValueKind == JsonValueKind.Object ? values.RootElement : default;
        var errors = new List<IError>();

        var ctx = new FieldValidationContext
        {
            Engine = this,
            PreloadedSchemas = preloadedSchemas != null ? new Dictionary<Guid, JsonDocument>(preloadedSchemas) : new()
        };

        foreach (var field in fields)
        {
            if (string.IsNullOrEmpty(field.Name)) continue;

            JsonElement fieldValue = default;
            if (valueObj.ValueKind == JsonValueKind.Object)
            {
                valueObj.TryGetProperty(field.Name, out fieldValue);
            }

            var processor = GetProcessor(field.Type);
            var result = processor.Validate(field, fieldValue, ctx);
            if (result.IsFailed)
            {
                errors.AddRange(result.Errors);
            }
        }

        return errors.Count > 0 ? Result.Fail(errors) : Result.Ok();
    }

    public JsonDocument NormalizeValues(
        JsonDocument schemaFields,
        JsonDocument values,
        IDictionary<Guid, JsonDocument>? preloadedSchemas = null)
    {
        var fields = ParseFields(schemaFields);
        var valueObj = values.RootElement.ValueKind == JsonValueKind.Object ? values.RootElement : default;
        var normalizedNode = new JsonObject();

        foreach (var field in fields)
        {
            if (string.IsNullOrEmpty(field.Name)) continue;

            JsonElement? fieldValue = null;
            if (valueObj.ValueKind == JsonValueKind.Object && valueObj.TryGetProperty(field.Name, out var v))
            {
                fieldValue = v;
            }

            var processor = GetProcessor(field.Type);
            var normalizedValue = processor.Normalize(field, fieldValue);
            normalizedNode[field.Name] = normalizedValue;
        }

        return JsonDocument.Parse(normalizedNode.ToJsonString());
    }

    public async Task<Result> LinkFileFieldsAsync(
        string schemaDataId,
        JsonDocument schemaFields,
        JsonDocument values,
        CancellationToken ct = default,
        IDictionary<Guid, JsonDocument>? preloadedSchemas = null)
    {
        var fields = ParseFields(schemaFields);
        var valueObj = values.RootElement.ValueKind == JsonValueKind.Object ? values.RootElement : default;
        if (valueObj.ValueKind != JsonValueKind.Object) return Result.Ok();

        var ctx = new FieldSaveContext
        {
            SchemaDataId = schemaDataId,
            Engine = this,
            PreloadedSchemas = preloadedSchemas != null ? new Dictionary<Guid, JsonDocument>(preloadedSchemas) : new()
        };

        foreach (var field in fields)
        {
            if (string.IsNullOrEmpty(field.Name)) continue;

            if (valueObj.TryGetProperty(field.Name, out var fieldValue))
            {
                var processor = GetProcessor(field.Type);
                var saveResult = await processor.BeforeSaveAsync(field, fieldValue, ctx, ct);
                if (saveResult.IsFailed)
                {
                    return saveResult;
                }
            }
        }

        return Result.Ok();
    }

    public async Task<Result<JsonDocument>> ResolveDataAsync(
        string schemaDataId,
        JsonDocument schemaFields,
        JsonDocument values,
        CancellationToken ct = default,
        IDictionary<Guid, JsonDocument>? preloadedSchemas = null)
    {
        var fields = ParseFields(schemaFields);
        var valueObj = values.RootElement.ValueKind == JsonValueKind.Object ? values.RootElement : default;
        var resolvedNode = new JsonObject();

        var ctx = new FieldResolveContext
        {
            SchemaDataId = schemaDataId,
            Engine = this,
            PreloadedSchemas = preloadedSchemas != null ? new Dictionary<Guid, JsonDocument>(preloadedSchemas) : new()
        };

        foreach (var field in fields)
        {
            if (string.IsNullOrEmpty(field.Name)) continue;

            if (valueObj.ValueKind == JsonValueKind.Object && valueObj.TryGetProperty(field.Name, out var fieldValue))
            {
                var processor = GetProcessor(field.Type);
                var resolved = await processor.ResolveAsync(field, fieldValue, ctx, ct);
                resolvedNode[field.Name] = resolved;
            }
            else
            {
                resolvedNode[field.Name] = null;
            }
        }

        return JsonDocument.Parse(resolvedNode.ToJsonString());
    }

    private IFieldTypeProcessor GetProcessor(string fieldType)
    {
        if (!string.IsNullOrEmpty(fieldType) && _processors.TryGetValue(fieldType, out var processor))
        {
            return processor;
        }

        return _defaultProcessor;
    }

    private static List<FieldDefinitionContext> ParseFields(JsonDocument schemaFields)
    {
        if (schemaFields == null || schemaFields.RootElement.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return schemaFields.RootElement
            .EnumerateArray()
            .Select(FieldDefinitionContext.FromJson)
            .ToList();
    }
}
