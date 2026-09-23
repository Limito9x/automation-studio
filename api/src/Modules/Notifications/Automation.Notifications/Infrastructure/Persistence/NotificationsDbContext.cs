using Automation.SharedKernel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Automation.Notifications.Domain.Entities;

namespace Automation.Notifications.Infrastructure.Persistence;

public class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : DbContext(options)
{
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("notifications");
        
        modelBuilder.Entity<Notification>(b =>
        {
            b.Property(x => x.Data).HasColumnType("jsonb");
        });
        
        modelBuilder.ApplySharedKernelConfigurations();
        
        base.OnModelCreating(modelBuilder);
    }
}



