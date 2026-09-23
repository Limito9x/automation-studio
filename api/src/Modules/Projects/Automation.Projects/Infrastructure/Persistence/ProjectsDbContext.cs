using Automation.SharedKernel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Automation.Projects.Domain;

namespace Automation.Projects.Infrastructure.Persistence;

public class ProjectsDbContext : DbContext
{
    public ProjectsDbContext(DbContextOptions<ProjectsDbContext> options) : base(options)
    {
    }

    public DbSet<Domain.Entities.Project> Projects => Set<Domain.Entities.Project>();
    public DbSet<Domain.Entities.ProjectMember> ProjectMembers => Set<Domain.Entities.ProjectMember>();
    public DbSet<Domain.Entities.ProjectExecutorConfig> ProjectExecutorConfigs => Set<Domain.Entities.ProjectExecutorConfig>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("projects");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProjectsDbContext).Assembly);
        modelBuilder.ApplySharedKernelConfigurations();
        base.OnModelCreating(modelBuilder);
    }
}

