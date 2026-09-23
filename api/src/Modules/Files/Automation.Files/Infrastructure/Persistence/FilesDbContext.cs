using Automation.Files.Domain.Entities;
using Automation.SharedKernel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Automation.Files.Infrastructure.Persistence;

public class FilesDbContext(DbContextOptions<FilesDbContext> options) : DbContext(options)
{
    public const string Schema = "files";

    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AssetLink> AssetLinks => Set<AssetLink>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FilesDbContext).Assembly);
        
        modelBuilder.ApplySharedKernelConfigurations();

        base.OnModelCreating(modelBuilder);
    }

}



