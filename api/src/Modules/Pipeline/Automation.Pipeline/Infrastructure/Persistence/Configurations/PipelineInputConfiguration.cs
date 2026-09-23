using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Pipeline.Infrastructure.Persistence.Configurations;

public class PipelineInputConfiguration : IEntityTypeConfiguration<Domain.Entities.PipelineInput>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.PipelineInput> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Pipeline)
            .WithMany(x => x.Inputs)
            .HasForeignKey(x => x.PipelineId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Label)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Cardinality)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.DefaultValue)
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.Property(x => x.Order)
            .IsRequired();

        builder.HasIndex(x => new { x.PipelineId, x.Key })
            .IsUnique();
    }
}
