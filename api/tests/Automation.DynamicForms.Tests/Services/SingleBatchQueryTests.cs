using System.Text.Json;
using Automation.DynamicForms.Constants;
using Automation.DynamicForms.Contracts;
using Automation.DynamicForms.Domain.Entities;
using Automation.DynamicForms.Infrastructure.Api;
using Automation.DynamicForms.Infrastructure.Persistence;
using Automation.DynamicForms.Services;
using Automation.DynamicForms.Services.Processors;
using Automation.Files.Contracts;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace Automation.DynamicForms.Tests.Services;

public class SingleBatchQueryTests
{
    private DynamicFormsDbContext CreateInMemoryDbContext(string dbName)
    {
        return TestDynamicFormsDbContext.Create(dbName);
    }

    private SchemaApi CreateSchemaApi(DynamicFormsDbContext db)
    {
        var assetApi = Substitute.For<IAssetApi>();
        var processors = new IFieldTypeProcessor[]
        {
            new DefaultFieldProcessor(),
            new FileFieldProcessor(assetApi),
            new StructFieldProcessor()
        };
        var engine = new DynamicFormEngine(processors);
        var registered = new[] { new RegisteredDynamicSchema(DynamicFormsOwnerType.ProjectStruct) };
        return new SchemaApi(db, registered, engine);
    }

    [Fact]
    public async Task GetActiveVersionWithDependenciesAsync_ShouldReturnAllNestedDependenciesInSingleBatch()
    {
        // Arrange: C -> B -> A (AssetManifest -> SlotBinding -> TextureConfig)
        using var db = CreateInMemoryDbContext(nameof(GetActiveVersionWithDependenciesAsync_ShouldReturnAllNestedDependenciesInSingleBatch));
        var api = CreateSchemaApi(db);

        var projectId = Guid.NewGuid().ToString();

        // 1. Struct C: TextureConfig
        var structC = new SchemaDefinition("TextureConfig", projectId, DynamicFormsOwnerType.ProjectStruct);
        var versionC = new SchemaVersion(structC.Id, JsonDocument.Parse("""[{"name":"diffuse","type":"text"}]"""), 1, true);
        db.SchemaDefinitions.Add(structC);
        db.SchemaVersions.Add(versionC);

        // 2. Struct B: SlotBinding (chứa C)
        var structB = new SchemaDefinition("SlotBinding", projectId, DynamicFormsOwnerType.ProjectStruct);
        var versionB = new SchemaVersion(structB.Id, JsonDocument.Parse($$$"""
        [
            {"name":"slotIndex","type":"number"},
            {"name":"texture","type":"struct","properties":{"structId":"{{{structC.Id}}}","cardinality":"single"}}
        ]
        """), 1, true)
        {
            DependencySchemaIds = [structC.Id]
        };
        db.SchemaDefinitions.Add(structB);
        db.SchemaVersions.Add(versionB);

        // 3. Struct A: AssetManifest (chứa B, transitive có cả C)
        var structA = new SchemaDefinition("AssetManifest", projectId, DynamicFormsOwnerType.ProjectStruct);
        var versionA = new SchemaVersion(structA.Id, JsonDocument.Parse($$$"""
        [
            {"name":"assetName","type":"text"},
            {"name":"slots","type":"struct","properties":{"structId":"{{{structB.Id}}}","cardinality":"array"}}
        ]
        """), 1, true)
        {
            DependencySchemaIds = [structB.Id, structC.Id]
        };
        db.SchemaDefinitions.Add(structA);
        db.SchemaVersions.Add(versionA);

        await db.SaveChangesAsync();

        // Act
        var result = await api.GetActiveVersionWithDependenciesAsync(structA.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value;
        dto.SchemaDefinitionId.Should().Be(structA.Id);
        dto.Name.Should().Be("AssetManifest");
        dto.ActiveVersion.Version.Should().Be(1);

        // Dependencies dictionary must contain exactly Struct B and Struct C
        dto.Dependencies.Should().HaveCount(2);
        dto.Dependencies.Should().ContainKey(structB.Id);
        dto.Dependencies.Should().ContainKey(structC.Id);

        dto.Dependencies[structB.Id].SchemaDefinitionId.Should().Be(structB.Id);
        dto.Dependencies[structB.Id].Fields.RootElement.GetArrayLength().Should().Be(2);

        dto.Dependencies[structC.Id].SchemaDefinitionId.Should().Be(structC.Id);
        dto.Dependencies[structC.Id].Fields.RootElement.GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task CanDeleteSchemaAsync_ShouldReturnFalseWhenReferenced_AndTrueWhenIndependent()
    {
        // Arrange
        using var db = CreateInMemoryDbContext(nameof(CanDeleteSchemaAsync_ShouldReturnFalseWhenReferenced_AndTrueWhenIndependent));
        var api = CreateSchemaApi(db);

        var projectId = Guid.NewGuid().ToString();

        var structChild = new SchemaDefinition("ChildStruct", projectId, DynamicFormsOwnerType.ProjectStruct);
        var versionChild = new SchemaVersion(structChild.Id, JsonDocument.Parse("[]"), 1, true);
        db.SchemaDefinitions.Add(structChild);
        db.SchemaVersions.Add(versionChild);

        var structParent = new SchemaDefinition("ParentStruct", projectId, DynamicFormsOwnerType.ProjectStruct);
        var versionParent = new SchemaVersion(structParent.Id, JsonDocument.Parse("[]"), 1, true)
        {
            DependencySchemaIds = [structChild.Id]
        };
        db.SchemaDefinitions.Add(structParent);
        db.SchemaVersions.Add(versionParent);

        await db.SaveChangesAsync();

        // Act & Assert
        var canDeleteChild = await api.CanDeleteSchemaAsync(structChild.Id);
        canDeleteChild.IsSuccess.Should().BeTrue();
        canDeleteChild.Value.Should().BeFalse(); // Parent is referencing it!

        var canDeleteParent = await api.CanDeleteSchemaAsync(structParent.Id);
        canDeleteParent.IsSuccess.Should().BeTrue();
        canDeleteParent.Value.Should().BeTrue(); // No one references parent!

        var referencingUsages = await api.GetReferencingSchemasAsync(structChild.Id);
        referencingUsages.IsSuccess.Should().BeTrue();
        referencingUsages.Value.Should().HaveCount(1);
        referencingUsages.Value[0].SchemaDefinitionId.Should().Be(structParent.Id);
        referencingUsages.Value[0].Name.Should().Be("ParentStruct");
    }
}
