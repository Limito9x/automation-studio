using Automation.SharedKernel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Automation.Tag.Domain.Entities;

namespace Automation.Tag.Infrastructure.Persistence;

public class TagDbContext : DbContext
{
    public TagDbContext(DbContextOptions<TagDbContext> options) : base(options)
    {
    }

    public DbSet<TagItem> TagItems => Set<TagItem>();
    public DbSet<TagLink> TagLinks => Set<TagLink>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("tag");
        modelBuilder.HasPostgresExtension("ltree");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TagDbContext).Assembly);
        modelBuilder.ApplySharedKernelConfigurations();
        base.OnModelCreating(modelBuilder);
    }
}