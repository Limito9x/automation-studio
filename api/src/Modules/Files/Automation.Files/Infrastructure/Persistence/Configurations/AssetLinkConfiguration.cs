using Automation.Files.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Files.Infrastructure.Persistence.Configurations;

public class AssetLinkConfiguration : IEntityTypeConfiguration<AssetLink>
{
    public void Configure(EntityTypeBuilder<AssetLink> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.OwnerEntityType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.OwnerEntityId).IsRequired().HasMaxLength(100);
        builder.Property(x => x.SlotKey).IsRequired().HasMaxLength(100);
        builder.Property(x => x.OriginalName).HasMaxLength(500);

        builder.HasIndex(x => new { x.OwnerEntityType, x.OwnerEntityId, x.SlotKey });

        builder.HasOne(x => x.Asset)
            .WithMany()
            .HasForeignKey(x => x.AssetId)
            .OnDelete(DeleteBehavior.Restrict); // Important: don't delete asset when deleting link automatically, storage needs to be cleaned up first
    }
}



