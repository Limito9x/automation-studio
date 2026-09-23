using Automation.SharedKernel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Automation.Workspace.Domain;

namespace Automation.Workspace.Infrastructure.Persistence;

public class WorkspaceDbContext : DbContext
{
    public WorkspaceDbContext(DbContextOptions<WorkspaceDbContext> options) : base(options)
    {
    }

    public DbSet<Domain.Entities.Workspace> Workspaces => Set<Domain.Entities.Workspace>();
    public DbSet<Domain.Entities.WorkspaceAgent> WorkspaceAgents => Set<Domain.Entities.WorkspaceAgent>();
    public DbSet<Domain.Entities.ResourceItem> ResourceItems => Set<Domain.Entities.ResourceItem>();
    public DbSet<Domain.Entities.ResourceVersion> ResourceVersions => Set<Domain.Entities.ResourceVersion>();
    public DbSet<Domain.Entities.ResourceVersionLocation> ResourceVersionLocations => Set<Domain.Entities.ResourceVersionLocation>();
    public DbSet<Domain.Entities.WorkspacePlatform> WorkspacePlatforms => Set<Domain.Entities.WorkspacePlatform>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("workspace");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkspaceDbContext).Assembly);
        modelBuilder.ApplySharedKernelConfigurations();
        base.OnModelCreating(modelBuilder);
    }
}

