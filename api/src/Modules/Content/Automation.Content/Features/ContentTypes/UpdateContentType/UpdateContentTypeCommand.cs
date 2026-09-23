using System.Text.Json;

namespace Automation.Content.Features.ContentTypes.UpdateContentType;

public record UpdateContentTypeCommand{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = null!;
    
    public string DisplayName { get; set; } = null!;
    
    public string? Description { get; set; }
    
    public string? Icon { get; set; }
    
    public string? Color { get; set; }
    
    public int SortOrder { get; set; }
    
    public JsonDocument? DisplayConfig { get; set; }  
};

