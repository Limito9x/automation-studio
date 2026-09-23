namespace Automation.SharedKernel.Domain.Interfaces;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt => DateTimeOffset.UtcNow;
}


