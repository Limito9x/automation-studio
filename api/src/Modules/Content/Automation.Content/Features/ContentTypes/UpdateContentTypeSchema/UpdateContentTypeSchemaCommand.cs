using System.Text.Json;
using System.Text.Json.Serialization;

namespace Automation.Content.Features.ContentTypes.UpdateContentTypeSchema;

public record UpdateContentTypeSchemaCommand{
    public Guid Id { get; set; }
    
    public JsonDocument? FieldsConfig { get; set; }
};

