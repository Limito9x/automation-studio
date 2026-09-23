namespace Automation.Platform.Domain.Entities;

public class PlatformExtension : AuditableEntity
{
    public string Extension { get; set; } = string.Empty;
    public ICollection<Platform> Platforms { get; set; } = new List<Platform>();

    public PlatformExtension() { }

    public PlatformExtension(string extension)
    {
        Extension = extension;
    }
}

