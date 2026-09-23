using Automation.SystemAbstractions;
using Automation.SystemModule.Domain.Entities;
using Automation.SystemModule.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Automation.SystemModule.Infrastructure;

public class SystemSettingsInitializer(
    IServiceProvider serviceProvider,
    ILogger<SystemSettingsInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        
        var registry = scope.ServiceProvider.GetRequiredService<ISystemSettingsRegistry>();
        var db = scope.ServiceProvider.GetRequiredService<SystemDbContext>();

        var registeredSettings = registry.GetAllSettings().ToList();
        if (registeredSettings.Count == 0)
        {
            return;
        }

        var existingKeys = await db.SystemSettings
            .Select(x => x.Key)
            .ToListAsync(cancellationToken);

        var settingsToAdd = registeredSettings
            .Where(r => !existingKeys.Contains(r.Key))
            .Select(r => new SystemSetting(r.Key, r.DefaultValue, r.ValueType, r.Description))
            .ToList();

        if (settingsToAdd.Count > 0)
        {
            logger.LogInformation("Seeding {Count} new system settings.", settingsToAdd.Count);
            db.SystemSettings.AddRange(settingsToAdd);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}



