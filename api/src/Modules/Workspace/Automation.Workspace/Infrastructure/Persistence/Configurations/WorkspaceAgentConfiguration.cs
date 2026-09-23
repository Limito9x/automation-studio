using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Workspace.Infrastructure.Persistence.Configurations;

public class WorkspaceAgentConfiguration : IEntityTypeConfiguration<Domain.Entities.WorkspaceAgent>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.WorkspaceAgent> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RootPath)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasOne(x => x.Workspace)
            .WithMany(x => x.WorkspaceAgents)
            .HasForeignKey(x => x.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.WorkspaceId, x.AgentId })
            .IsUnique();
    }
}
