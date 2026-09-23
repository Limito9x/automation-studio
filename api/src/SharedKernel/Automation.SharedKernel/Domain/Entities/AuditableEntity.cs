using Automation.SharedKernel.Domain.Interfaces;

namespace Automation.SharedKernel.Domain.Entities;

public abstract class AuditableEntity<TId> : Entity<TId>, IAuditable where TId : notnull
{
    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

public abstract class AuditableEntity : AuditableEntity<Guid>
{
    public AuditableEntity()
    {
        Id = IdGenerator.NewId();
    }

    public AuditableEntity(Guid id)
    {
        Id = id != Guid.Empty ? id : IdGenerator.NewId();
    }
}
