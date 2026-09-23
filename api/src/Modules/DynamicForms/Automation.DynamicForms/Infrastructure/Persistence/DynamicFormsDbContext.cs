using Automation.SharedKernel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Automation.DynamicForms.Domain.Entities;

namespace Automation.DynamicForms.Infrastructure.Persistence;

public class DynamicFormsDbContext : DbContext
{
    public DbSet<SchemaDefinition> SchemaDefinitions { get; set; } = null!;
    public DbSet<SchemaVersion> SchemaVersions { get; set; } = null!;
    public DbSet<SchemaData> SchemaData { get; set; } = null!;

    public DynamicFormsDbContext(DbContextOptions<DynamicFormsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dynamicforms");
        
        modelBuilder.Entity<SchemaDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.OwnerId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.OwnerType).IsRequired().HasMaxLength(100);
            
            entity.HasIndex(e => new { e.OwnerId, e.OwnerType, e.Name }).IsUnique();
            
            entity.HasMany(e => e.Versions)
                  .WithOne(e => e.SchemaDefinition)
                  .HasForeignKey(e => e.SchemaDefinitionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SchemaVersion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Fields).HasColumnType("jsonb").IsRequired();
            entity.Property(e => e.Version).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.DependencySchemaIds)
                  .HasDefaultValueSql("'{}'::uuid[]")
                  .IsRequired();
        });

        modelBuilder.Entity<SchemaData>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Values).HasColumnType("jsonb").IsRequired();
            entity.Property(e => e.ClientId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ClientType).IsRequired().HasMaxLength(100);
            
            entity.HasIndex(e => new { e.ClientId, e.ClientType });
            
            entity.HasOne(e => e.SchemaVersion)
                  .WithMany()
                  .HasForeignKey(e => e.SchemaVersionId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(modelBuilder);
    }
}

