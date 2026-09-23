using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Content.Infrastructure.Persistence.Configurations;

public class ContentTypeConfiguration : IEntityTypeConfiguration<Domain.Entities.ContentType>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.ContentType> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.DisplayName)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(x => x.Description)
            .HasMaxLength(1000);
            
        builder.Property(x => x.Icon)
            .HasMaxLength(100);
            
        builder.Property(x => x.Color)
            .HasMaxLength(50);
            
        builder.Property(x => x.SortOrder)
            .IsRequired();
            
        builder.Property(x => x.DisplayConfig)
            .HasColumnType("jsonb")
            .IsRequired();
            
        builder.HasIndex(x => new { x.ProjectId, x.Key }).IsUnique();
    }
}

