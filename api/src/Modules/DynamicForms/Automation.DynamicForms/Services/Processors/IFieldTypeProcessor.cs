using System.Text.Json;
using System.Text.Json.Nodes;

namespace Automation.DynamicForms.Services.Processors;

public interface IFieldTypeProcessor
{
    string FieldType { get; }

    Result Validate(FieldDefinitionContext field, JsonElement value, FieldValidationContext ctx);

    JsonNode? Normalize(FieldDefinitionContext field, JsonElement? rawValue);

    Task<Result> BeforeSaveAsync(FieldDefinitionContext field, JsonElement value, FieldSaveContext ctx, CancellationToken ct = default);

    Task<JsonNode?> ResolveAsync(FieldDefinitionContext field, JsonElement value, FieldResolveContext ctx, CancellationToken ct = default);
}
