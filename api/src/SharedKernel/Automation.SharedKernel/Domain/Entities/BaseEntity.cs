namespace Automation.SharedKernel.Domain.Entities;

public abstract class BaseEntity<TId> : SoftDeleteEntity<TId> where TId : notnull
{
}

public abstract class BaseEntity : BaseEntity<Guid>
{
    public BaseEntity()
    {
        Id = IdGenerator.NewId();
    }

    public BaseEntity(Guid id)
    {
        Id = id != Guid.Empty ? id : IdGenerator.NewId();
    }
}
