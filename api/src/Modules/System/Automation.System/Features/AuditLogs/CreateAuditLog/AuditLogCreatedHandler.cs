using Automation.SharedKernel.Domain.Events;
using Automation.SystemModule.Domain.Entities;
using Automation.SystemModule.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace Automation.SystemModule.Features.AuditLogs.CreateAuditLog;

public static class AuditLogCreatedHandler
{
    public static async Task Handle(
     AuditLogCreatedMessage message,
      SystemDbContext dbContext,
       CancellationToken cancellationToken)
    {
        Console.WriteLine($"AuditLogCreatedHandler fired for {message.EntityName}");

        var auditLog = new AuditLog
        {
            UserId = message.UserId,
            Action = message.Action,
            EntityName = message.EntityName,
            EntityId = message.EntityId,
            OldValues = message.OldValues,
            NewValues = message.NewValues,
            Timestamp = message.Timestamp,
            IpAddress = message.IpAddress,
            UserAgent = message.UserAgent
        };

        dbContext.AuditLogs.Add(auditLog);
        
        // Save changes without triggering auditing on itself!
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}



