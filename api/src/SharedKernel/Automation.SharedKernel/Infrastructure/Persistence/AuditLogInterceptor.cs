using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Automation.SharedKernel.Domain.Events;
using Automation.SharedKernel.Domain.Interfaces;
using Wolverine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Automation.SharedKernel.Infrastructure.Persistence;

public sealed class AuditLogInterceptor(
    ILogger<AuditLogInterceptor> logger,
    IHttpContextAccessor httpContextAccessor,
    ICurrentUserProvider currentUser,
    IServiceScopeFactory scopeFactory) : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions _auditJsonOptions = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null) return result;

        var context = eventData.Context;
        
        var ipAddress = httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        var userAgent = httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();
        var userId = currentUser.UserId;

        var auditMessages = new List<AuditLogCreatedMessage>();

        foreach (var entry in context.ChangeTracker.Entries<IAuditTrackable>())
        {
            if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var ignoredProperties = GetIgnoredProperties(entry.Entity.GetType());
            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();
            var action = entry.State.ToString();

            if (entry.State == EntityState.Added)
            {
                action = "Created";
                foreach (var prop in entry.Properties)
                {
                    if (!prop.IsTemporary && !ignoredProperties.Contains(prop.Metadata.Name))
                        newValues[prop.Metadata.Name] = prop.CurrentValue;
                }
            }
            else if (entry.State == EntityState.Deleted)
            {
                action = "Deleted";
                foreach (var prop in entry.Properties)
                {
                    if (!ignoredProperties.Contains(prop.Metadata.Name))
                        oldValues[prop.Metadata.Name] = prop.OriginalValue;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                action = "Updated";
                foreach (var prop in entry.Properties)
                {
                    if (prop.IsModified && !ignoredProperties.Contains(prop.Metadata.Name))
                    {
                        oldValues[prop.Metadata.Name] = prop.OriginalValue;
                        newValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }
            }

            if (oldValues.Count == 0 && newValues.Count == 0)
                continue;

            var idProperty = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
            var entityId = idProperty?.CurrentValue?.ToString() ?? "Unknown";

            var auditMessage = new AuditLogCreatedMessage(
                UserId: userId.ToString(),
                Action: action,
                EntityName: entry.Entity.GetType().Name,
                EntityId: entityId,
                OldValues: oldValues.Count > 0 ? JsonSerializer.Serialize(oldValues, _auditJsonOptions) : null,
                NewValues: newValues.Count > 0 ? JsonSerializer.Serialize(newValues, _auditJsonOptions) : null,
                Timestamp: DateTimeOffset.UtcNow,
                IpAddress: ipAddress,
                UserAgent: userAgent
            );

            auditMessages.Add(auditMessage);
        }

        if (auditMessages.Count == 0)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        // Tạo scope để resolve IMessageBus (Scoped service) an toàn từ Singleton interceptor
        await using var scope = scopeFactory.CreateAsyncScope();
        var messageBus = scope.ServiceProvider.GetService<IMessageBus>();

        if (messageBus is null)
        {
            logger.LogWarning("IMessageBus not available — audit messages will not be published ({Count} messages skipped)", auditMessages.Count);
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        logger.LogInformation("Publishing {Count} audit log messages", auditMessages.Count);
        foreach (var message in auditMessages)
        {
            try
            {
                logger.LogInformation("Publishing audit message: {EntityName} {Action} {EntityId}", message.EntityName, message.Action, message.EntityId);
                await messageBus.PublishAsync(message);
            }
            catch (Wolverine.WolverineHasNotStartedException)
            {
                logger.LogWarning("Wolverine has not started yet. Skipping audit log for {EntityName} {Action} {EntityId}", message.EntityName, message.Action, message.EntityId);
            }
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static readonly Dictionary<Type, HashSet<string>> _ignoredPropertiesCache = new();

    private static HashSet<string> GetIgnoredProperties(Type entityType)
    {
        if (_ignoredPropertiesCache.TryGetValue(entityType, out var cached))
            return cached;

        var ignored = entityType.GetProperties()
            .Where(p => p.GetCustomAttribute<AuditIgnoreAttribute>() != null)
            .Select(p => p.Name)
            .ToHashSet();

        _ignoredPropertiesCache[entityType] = ignored;
        return ignored;
    }
}
