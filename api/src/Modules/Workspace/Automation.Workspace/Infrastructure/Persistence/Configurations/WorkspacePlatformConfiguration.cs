using Automation.Workspace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Workspace.Infrastructure.Persistence.Configurations;

public class WorkspacePlatformConfiguration : IEntityTypeConfiguration<WorkspacePlatform>
{
    public void Configure(EntityTypeBuilder<WorkspacePlatform> builder)
    {
        builder.ToTable("WorkspacePlatforms");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.WorkspaceId, x.PlatformId }).IsUnique();

        builder.HasOne(x => x.Workspace)
            .WithMany(x => x.WorkspacePlatforms)
            .HasForeignKey(x => x.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
