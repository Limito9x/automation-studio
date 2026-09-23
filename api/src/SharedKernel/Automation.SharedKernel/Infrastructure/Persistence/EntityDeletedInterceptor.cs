using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Automation.SharedKernel.Domain.Events;
using Automation.SharedKernel.Domain.Interfaces;
using Wolverine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Automation.SharedKernel.Infrastructure.Persistence;

public sealed class EntityDeletedInterceptor(
    ILogger<EntityDeletedInterceptor> logger,
    IServiceScopeFactory scopeFactory) : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null) return await base.SavingChangesAsync(eventData, result, cancellationToken);

        var context = eventData.Context;
        var deletedMessages = new List<EntityDeletedMessage>();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            var isHardDeleted = entry.State == EntityState.Deleted;
            var isSoftDeleted = entry.Entity is ISoftDelete softDelete &&
                                (entry.State == EntityState.Deleted || 
                                 (entry.State == EntityState.Modified && softDelete.DeletedAt != null));

            if (!isHardDeleted && !isSoftDeleted)
                continue;

            var idProperty = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
            var entityId = idProperty?.CurrentValue?.ToString()
                        ?? idProperty?.OriginalValue?.ToString();

            if (string.IsNullOrEmpty(entityId))
                continue;

            var entityTypeName = entry.Metadata.ClrType.Name;
            deletedMessages.Add(new EntityDeletedMessage(entityTypeName, entityId));
        }

        if (deletedMessages.Count == 0)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        await using var scope = scopeFactory.CreateAsyncScope();
        var messageBus = scope.ServiceProvider.GetService<IMessageBus>();

        if (messageBus != null)
        {
            foreach (var message in deletedMessages)
            {
                try
                {
                    logger.LogInformation("Publishing EntityDeletedMessage for {OwnerEntityType} (ID: {OwnerEntityId})",
                        message.OwnerEntityType, message.OwnerEntityId);
                    await messageBus.PublishAsync(message);
                }
                catch (Wolverine.WolverineHasNotStartedException)
                {
                    logger.LogWarning("Wolverine has not started yet. Skipping EntityDeletedMessage for {OwnerEntityType} (ID: {OwnerEntityId})",
                        message.OwnerEntityType, message.OwnerEntityId);
                }
            }
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}

