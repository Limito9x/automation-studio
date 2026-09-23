namespace Automation.Platform.Domain.Entities;

public class Platform : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ICollection<PlatformExtension> Extensions { get; set; } = new List<PlatformExtension>();

    public Platform() { }

    public Platform(string key, string name)
    {
        Key = key;
        Name = name;
    }

    public void Update(string name)
    {
        Name = name;
    }

    public void SetExtensions(IEnumerable<PlatformExtension> extensions)
    {
        Extensions.Clear();
        foreach (var ext in extensions)
        {
            Extensions.Add(ext);
        }
    }
}

