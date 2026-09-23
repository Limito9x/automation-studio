using Automation.Tag.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Tag.Infrastructure.Persistence.Configurations;

public class TagLinkConfiguration : IEntityTypeConfiguration<TagLink>
{
    public void Configure(EntityTypeBuilder<TagLink> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Tag)
            .WithMany(x => x.Links)
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.EntityId)
            .IsRequired();

        builder.Property(x => x.TagPath)
            .IsRequired();

        builder.Property(x => x.TargetSubPath)
            .HasMaxLength(255);

        builder.Property(x => x.Metadata)
            .HasColumnType("jsonb");

        builder.Property(x => x.ProjectId)
            .IsRequired();

        builder.HasIndex(x => x.ProjectId);
        builder.HasIndex(x => new { x.EntityType, x.EntityId });
        builder.HasIndex(x => new { x.ProjectId, x.EntityType, x.EntityId });
        builder.HasIndex(x => x.TagPath).HasMethod("gist");
    }
}
