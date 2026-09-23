using System.Text.Json;
using System.Text.Json.Serialization;
using Automation.Pipeline.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Pipeline.Infrastructure.Persistence.Configurations;

public class PipelineConfiguration : IEntityTypeConfiguration<Domain.Entities.Pipeline>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Pipeline> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(255);

        builder.Property(x => x.TriggerType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.TriggerWorkspaceId)
            .IsRequired(false);

        builder.Property(x => x.TriggerConfig)
            .HasColumnType("jsonb")
            .IsRequired(false);

        builder.HasIndex(x => new { x.ProjectId, x.Name })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        var jsonOptions = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() }
        };

        builder.Property(x => x.Variables)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v ?? new List<PipelineVariableDecl>(), jsonOptions),
                v => SafeDeserializeVariables(v, jsonOptions)
            )
            .Metadata.SetValueComparer(new ValueComparer<List<PipelineVariableDecl>>(
                (c1, c2) => JsonSerializer.Serialize(c1, jsonOptions) == JsonSerializer.Serialize(c2, jsonOptions),
                c => c == null ? 0 : JsonSerializer.Serialize(c, jsonOptions).GetHashCode(),
                c => SafeDeserializeVariables(JsonSerializer.Serialize(c, jsonOptions), jsonOptions)
            ));
    }

    private static List<PipelineVariableDecl> SafeDeserializeVariables(string? json, JsonSerializerOptions options)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<PipelineVariableDecl>();
        }

        try
        {
            var trimmed = json.Trim();
            if (trimmed.StartsWith("["))
            {
                return JsonSerializer.Deserialize<List<PipelineVariableDecl>>(trimmed, options) ?? new List<PipelineVariableDecl>();
            }
        }
        catch
        {
            // Ignore corrupted json and fallback to empty list
        }

        return new List<PipelineVariableDecl>();
    }
}
