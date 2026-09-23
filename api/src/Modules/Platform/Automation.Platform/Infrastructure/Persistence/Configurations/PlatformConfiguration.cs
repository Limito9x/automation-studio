using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Platform.Infrastructure.Persistence.Configurations;

public class PlatformConfiguration : IEntityTypeConfiguration<Domain.Entities.Platform>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Platform> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.HasIndex(x => x.Key)
            .IsUnique();
    }
}

