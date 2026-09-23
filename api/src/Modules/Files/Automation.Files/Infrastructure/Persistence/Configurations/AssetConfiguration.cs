using Automation.Files.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Files.Infrastructure.Persistence.Configurations;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.StoragePath).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.ContentType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Extension).IsRequired().HasMaxLength(20);
        builder.Property(x => x.HashSha256).HasMaxLength(64);
        
        builder.HasIndex(x => x.StoragePath).IsUnique();
        builder.HasIndex(x => x.HashSha256).IsUnique();
    }
}



