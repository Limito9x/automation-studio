using Microsoft.EntityFrameworkCore;
using Automation.SharedKernel.Infrastructure.Persistence;
using Automation.SystemModule.Domain.Entities;

namespace Automation.SystemModule.Infrastructure.Persistence;

public class SystemDbContext(DbContextOptions<SystemDbContext> options) : DbContext(options)
{
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.HasDefaultSchema("system");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SystemDbContext).Assembly);
        
        modelBuilder.ApplySharedKernelConfigurations();
    }
}



