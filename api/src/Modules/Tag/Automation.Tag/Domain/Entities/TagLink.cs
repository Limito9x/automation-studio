using System.Text.Json;

namespace Automation.Tag.Domain.Entities;

public class TagLink : AuditableEntity
{
    public Guid ProjectId { get; set; }
    public Guid TagId { get; set; }
    public TagItem Tag { get; set; } = null!;
    
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    
    /// <summary>
    /// Denormalized ltree tag path for high-performance hierarchical querying without joins
    /// </summary>
    public LTree TagPath { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional sub-path inside entity (e.g. mesh name, material index, json path)
    /// </summary>
    public string? TargetSubPath { get; set; }
    
    public JsonDocument? Metadata { get; set; }

    public TagLink() { }

    public TagLink(
        Guid projectId,
        Guid tagId,
        string tagPath,
        string entityType,
        Guid entityId,
        string? targetSubPath = null,
        JsonDocument? metadata = null
    )
    {
        ProjectId = projectId;
        TagId = tagId;
        TagPath = tagPath;
        EntityType = entityType;
        EntityId = entityId;
        TargetSubPath = targetSubPath;
        Metadata = metadata;
    }
}
