using Automation.SharedKernel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Automation.Platform.Domain;

namespace Automation.Platform.Infrastructure.Persistence;

public class PlatformDbContext : DbContext
{
    public PlatformDbContext(DbContextOptions<PlatformDbContext> options) : base(options)
    {
    }

    public DbSet<Domain.Entities.Platform> Platforms => Set<Domain.Entities.Platform>();
    public DbSet<Domain.Entities.PlatformExtension> PlatformExtensions => Set<Domain.Entities.PlatformExtension>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("platform");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlatformDbContext).Assembly);
        modelBuilder.ApplySharedKernelConfigurations();
        base.OnModelCreating(modelBuilder);
    }
}

