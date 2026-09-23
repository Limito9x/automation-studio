using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Platform.Infrastructure.Persistence.Configurations;

public class PlatformExtensionConfiguration : IEntityTypeConfiguration<Domain.Entities.PlatformExtension>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.PlatformExtension> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Extension)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.Extension)
            .IsUnique();

        builder.HasMany(x => x.Platforms)
            .WithMany(x => x.Extensions);
    }
}

