using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Agent.Infrastructure.Persistence.Configurations;

public class AgentConfiguration : IEntityTypeConfiguration<Domain.Entities.Agent>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Agent> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.MachineKey)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.RegistrationToken)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(x => x.MachineKey)
            .IsUnique();

        builder.HasIndex(x => x.RegistrationToken)
            .IsUnique();

        builder.HasMany(x => x.ExecutorConfigs)
            .WithOne(x => x.Agent)
            .HasForeignKey(x => x.AgentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

