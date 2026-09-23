using System.Text.Json;
using System.Text.Json.Nodes;

namespace Automation.DynamicForms.Services.Processors;

public class FieldDefinitionContext
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public JsonElement Properties { get; set; }
    public JsonElement RawElement { get; set; }

    public bool IsRequired => Properties.ValueKind == JsonValueKind.Object &&
                              Properties.TryGetProperty("required", out var req) && req.GetBoolean();

    public string RequiredMessage => Properties.ValueKind == JsonValueKind.Object &&
                                     Properties.TryGetProperty("requiredMsg", out var msg) && msg.GetString() is { } s ? s : $"{Name} is required";

    public string Cardinality => Properties.ValueKind == JsonValueKind.Object &&
                                 Properties.TryGetProperty("cardinality", out var c) && c.GetString() is { } s ? s : "single";

    public Guid? StructId => Properties.ValueKind == JsonValueKind.Object &&
                             Properties.TryGetProperty("structId", out var id) &&
                             Guid.TryParse(id.GetString(), out var g) && g != Guid.Empty ? g : null;

    public static FieldDefinitionContext FromJson(JsonElement element)
    {
        var name = element.TryGetProperty("name", out var n) && n.ValueKind == JsonValueKind.String ? n.GetString() ?? string.Empty : string.Empty;
        var type = element.TryGetProperty("type", out var t) && t.ValueKind == JsonValueKind.String ? t.GetString() ?? string.Empty : string.Empty;
        var props = element.TryGetProperty("properties", out var p) && p.ValueKind == JsonValueKind.Object ? p : default;

        return new FieldDefinitionContext
        {
            Name = name,
            Type = type,
            Properties = props,
            RawElement = element
        };
    }
}

public class FieldValidationContext
{
    public IDynamicFormEngine? Engine { get; set; }
    public Dictionary<Guid, JsonDocument> PreloadedSchemas { get; set; } = new();
}

public class FieldSaveContext
{
    public string SchemaDataId { get; set; } = string.Empty;
    public IDynamicFormEngine? Engine { get; set; }
    public Dictionary<Guid, JsonDocument> PreloadedSchemas { get; set; } = new();
}

public class FieldResolveContext
{
    public string SchemaDataId { get; set; } = string.Empty;
    public IDynamicFormEngine? Engine { get; set; }
    public Dictionary<Guid, JsonDocument> PreloadedSchemas { get; set; } = new();
}
