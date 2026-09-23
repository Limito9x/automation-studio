using Automation.SharedKernel.Domain.Interfaces;

namespace Automation.SharedKernel.Domain.Entities;

public abstract class SoftDeleteEntity<TId> : AuditableEntity<TId>, ISoftDelete where TId : notnull
{
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool IsDeleted => DeletedAt.HasValue;
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

public abstract class SoftDeleteEntity : SoftDeleteEntity<Guid>
{
    public SoftDeleteEntity()
    {
        Id = IdGenerator.NewId();
    }

    public SoftDeleteEntity(Guid id)
    {
        Id = id != Guid.Empty ? id : IdGenerator.NewId();
    }
}
