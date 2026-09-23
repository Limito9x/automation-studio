namespace Automation.SharedKernel.Domain.Entities;

public abstract class Entity<TId> where TId : notnull
{
    public TId Id { get; set; } = default!;

    public override bool Equals(object? obj) =>
        obj is Entity<TId> other && EqualityComparer<TId>.Default.Equals(Id, other.Id);

    public override int GetHashCode() => Id.GetHashCode();
}

public abstract class Entity : Entity<Guid>
{
    public Entity()
    {
        Id = IdGenerator.NewId();
    }

    public Entity(Guid id)
    {
        Id = id != Guid.Empty ? id : IdGenerator.NewId();
    }
}
