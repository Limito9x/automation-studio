namespace Automation.SharedKernel.Domain.Events;

public record EntityDeletedMessage(
    string OwnerEntityType,
    string OwnerEntityId
);

