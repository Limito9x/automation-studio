using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Automation.SystemModule.Domain.Entities;

namespace Automation.SystemModule.Infrastructure.Persistence.Configurations;

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.ToTable("SystemSettings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key)
            .HasMaxLength(200)
            .IsRequired();
            
        builder.HasIndex(x => x.Key)
            .IsUnique();

        builder.Property(x => x.Value)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.ValueType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500);
    }
}



