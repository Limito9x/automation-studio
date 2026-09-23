using System.Text.Json;
using Automation.DynamicForms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Automation.DynamicForms.Tests;

public class TestDynamicFormsDbContext : DynamicFormsDbContext
{
    public TestDynamicFormsDbContext(DbContextOptions<DynamicFormsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var jsonConverter = new ValueConverter<JsonDocument, string>(
            v => v.RootElement.GetRawText(),
            v => JsonDocument.Parse(string.IsNullOrEmpty(v) ? "{}" : v, default));

        var guidListConverter = new ValueConverter<List<Guid>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<List<Guid>>(v, (JsonSerializerOptions?)null) ?? new List<Guid>());

        modelBuilder.Entity<Domain.Entities.SchemaVersion>(entity =>
        {
            entity.Property(e => e.Fields).HasConversion(jsonConverter);
            entity.Property(e => e.DependencySchemaIds).HasConversion(guidListConverter);
        });

        modelBuilder.Entity<Domain.Entities.SchemaData>(entity =>
        {
            entity.Property(e => e.Values).HasConversion(jsonConverter);
        });
    }

    public static TestDynamicFormsDbContext Create(string dbName)
    {
        var options = new DbContextOptionsBuilder<DynamicFormsDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new TestDynamicFormsDbContext(options);
    }
}
