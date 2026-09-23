using Automation.SharedKernel.Domain.Interfaces;

namespace Automation.SharedKernel.Domain.Entities;

public abstract class AggregateRoot<TId>: Entity<TId> where TId : notnull
{
    private readonly List<IDomainEvent> _events = [];
    public IReadOnlyList<IDomainEvent> DomainEvents => _events;
    protected void Raise(IDomainEvent @event) => _events.Add(@event);
    public void ClearEvents() => _events.Clear();
}


