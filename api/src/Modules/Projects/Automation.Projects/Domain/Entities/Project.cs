namespace Automation.Projects.Domain.Entities;

public class Project : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public Guid OwnerId { get; set; } = Guid.Empty;

    public Project() { }

    public Project(string name, Guid ownerId)
    {
        Name = name;
        OwnerId = ownerId;
    }

    public void Update(string name)
    {
        Name = name;
    }
}

