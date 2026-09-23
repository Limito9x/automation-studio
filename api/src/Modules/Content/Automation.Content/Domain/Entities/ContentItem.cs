using System.Text.Json;
using Automation.SharedKernel.Domain.Interfaces;

namespace Automation.Content.Domain.Entities;

public class ContentItem : BaseEntity, IAuditTrackable
{
    public Guid ContentTypeId { get; set; }
    public ContentType ContentType { get; set; } = null!;
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;

    public ContentItem() { }

    public ContentItem(Guid contentTypeId, Guid projectId, string name)
    {
        ContentTypeId = contentTypeId;
        ProjectId = projectId;
        Name = name;
    }

    public void Update(string name)
    {
        Name = name;
    }
}

