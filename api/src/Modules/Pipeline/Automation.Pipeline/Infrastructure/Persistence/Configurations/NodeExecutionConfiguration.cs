using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Pipeline.Infrastructure.Persistence.Configurations;

public class NodeExecutionConfiguration : IEntityTypeConfiguration<Domain.Entities.NodeExecution>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.NodeExecution> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasOne(x => x.PipelineExecution)
            .WithMany()
            .HasForeignKey(x => x.PipelineExecutionId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(x => x.PipelineNode)
            .WithMany()
            .HasForeignKey(x => x.PipelineNodeId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50);
            
        builder.Property(x => x.Progress)
            .HasColumnType("jsonb")
            .IsRequired(false);

        builder.Property(x => x.Output)
            .HasColumnType("jsonb")
            .IsRequired(false);

        builder.Property(x => x.Log)
            .HasColumnType("jsonb")
            .IsRequired(false);

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(2000)
            .IsRequired(false);
    }
}
