using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Content.Infrastructure.Persistence.Configurations;

public class ContentItemConfiguration : IEntityTypeConfiguration<Domain.Entities.ContentItem>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.ContentItem> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasOne(x => x.ContentType)
            .WithMany()
            .HasForeignKey(x => x.ContentTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.HasIndex(x => new { x.ProjectId, x.ContentTypeId, x.Name }).IsUnique();
    }
}

