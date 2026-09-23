using System.Text.Json;

namespace Automation.DynamicForms.Services;

public interface IDynamicFormEngine
{
    Result ValidateValues(
        JsonDocument schemaFields,
        JsonDocument values,
        IDictionary<Guid, JsonDocument>? preloadedSchemas = null);

    JsonDocument NormalizeValues(
        JsonDocument schemaFields,
        JsonDocument values,
        IDictionary<Guid, JsonDocument>? preloadedSchemas = null);

    Task<Result> LinkFileFieldsAsync(
        string schemaDataId,
        JsonDocument schemaFields,
        JsonDocument values,
        CancellationToken ct = default,
        IDictionary<Guid, JsonDocument>? preloadedSchemas = null);

    Task<Result<JsonDocument>> ResolveDataAsync(
        string schemaDataId,
        JsonDocument schemaFields,
        JsonDocument values,
        CancellationToken ct = default,
        IDictionary<Guid, JsonDocument>? preloadedSchemas = null);
}
