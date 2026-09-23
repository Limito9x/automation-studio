using System.Text.Json;
using Automation.DynamicForms.Domain.Entities;
using Automation.DynamicForms.Services;
using Automation.DynamicForms.Services.Processors;
using Automation.Files.Contracts;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Automation.DynamicForms.Tests.Services;

public class DynamicFormEngineTests
{
    private readonly IAssetApi _assetApi = Substitute.For<IAssetApi>();

    private DynamicFormEngine CreateEngine()
    {
        var processors = new IFieldTypeProcessor[]
        {
            new DefaultFieldProcessor(),
            new FileFieldProcessor(_assetApi),
            new StructFieldProcessor()
        };
        return new DynamicFormEngine(processors);
    }

    [Fact]
    public void ValidateValues_ShouldPass_WhenAllRequiredFieldsPresent()
    {
        // Arrange
        var engine = CreateEngine();
        var schemaFields = JsonDocument.Parse("""
        [
            { "name": "title", "type": "text", "properties": { "required": true } },
            { "name": "age", "type": "number", "properties": { "required": false } }
        ]
        """);

        var values = JsonDocument.Parse("""
        {
            "title": "Hello World",
            "age": 25
        }
        """);

        // Act
        var result = engine.ValidateValues(schemaFields, values);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateValues_ShouldFail_WhenRequiredFieldMissingOrEmpty()
    {
        // Arrange
        var engine = CreateEngine();
        var schemaFields = JsonDocument.Parse("""
        [
            { "name": "title", "type": "text", "properties": { "required": true, "requiredMsg": "Title is mandatory" } }
        ]
        """);

        var valuesEmpty = JsonDocument.Parse("""{ "title": "" }""");
        var valuesMissing = JsonDocument.Parse("""{}""");

        // Act
        var resultEmpty = engine.ValidateValues(schemaFields, valuesEmpty);
        var resultMissing = engine.ValidateValues(schemaFields, valuesMissing);

        // Assert
        resultEmpty.IsFailed.Should().BeTrue();
        resultEmpty.Errors[0].Message.Should().Be("Title is mandatory");

        resultMissing.IsFailed.Should().BeTrue();
        resultMissing.Errors[0].Message.Should().Be("Title is mandatory");
    }

    [Fact]
    public void NormalizeValues_ShouldAddMissingFieldsAsNull()
    {
        // Arrange
        var engine = CreateEngine();
        var schemaFields = JsonDocument.Parse("""
        [
            { "name": "title", "type": "text" },
            { "name": "description", "type": "text" }
        ]
        """);

        var values = JsonDocument.Parse("""{ "title": "Hello" }""");

        // Act
        var normalized = engine.NormalizeValues(schemaFields, values);

        // Assert
        var root = normalized.RootElement;
        root.GetProperty("title").GetString().Should().Be("Hello");
        root.GetProperty("description").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task LinkFileFieldsAsync_ShouldInvokeAssetApi_ForFileFields()
    {
        // Arrange
        var engine = CreateEngine();
        var assetId = Guid.NewGuid();

        var schemaFields = JsonDocument.Parse("""
        [
            { "name": "avatar", "type": "file-upload" }
        ]
        """);

        var values = JsonDocument.Parse($$"""
        {
            "avatar": { "assetId": "{{assetId}}", "name": "avatar.png" }
        }
        """);

        _assetApi.UpsertMultipleAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IEnumerable<AssetUpsertDto>>(),
            Arg.Any<CancellationToken>())
            .Returns(FluentResults.Result.Ok());

        // Act
        var result = await engine.LinkFileFieldsAsync("schema-data-123", schemaFields, values);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _assetApi.Received(1).UpsertMultipleAsync(
            nameof(SchemaData),
            "schema-data-123",
            Arg.Any<string>(),
            Arg.Is<IEnumerable<AssetUpsertDto>>(dtos => dtos.Any(d => d.AssetId == assetId)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ValidateValues_ShouldRecursivelyValidate_NestedStructField()
    {
        // Arrange
        var engine = CreateEngine();
        var childStructId = Guid.NewGuid();

        var childFields = JsonDocument.Parse("""
        [
            { "name": "slotName", "type": "text", "properties": { "required": true } }
        ]
        """);

        var preloaded = new Dictionary<Guid, JsonDocument>
        {
            [childStructId] = childFields
        };

        // Parent schema referencing child struct
        var parentSchema = JsonDocument.Parse($$"""
        [
            {
                "name": "slot",
                "type": "struct",
                "properties": {
                    "structId": "{{childStructId}}",
                    "cardinality": "single",
                    "required": true
                }
            }
        ]
        """);

        // Values with invalid child struct (missing required slotName)
        var invalidValues = JsonDocument.Parse("""
        {
            "slot": { "slotName": "" }
        }
        """);

        // Act
        var result = engine.ValidateValues(parentSchema, invalidValues, preloaded);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("slotName is required"));
    }

    [Fact]
    public void ValidateValues_ShouldRecursivelyValidate_ArrayOfStructs()
    {
        // Arrange
        var engine = CreateEngine();
        var skillStructId = Guid.NewGuid();

        var skillFields = JsonDocument.Parse("""
        [
            { "name": "damage", "type": "number", "properties": { "required": true } }
        ]
        """);

        var preloaded = new Dictionary<Guid, JsonDocument>
        {
            [skillStructId] = skillFields
        };

        var parentSchema = JsonDocument.Parse($$"""
        [
            {
                "name": "skills",
                "type": "struct",
                "properties": {
                    "structId": "{{skillStructId}}",
                    "cardinality": "array"
                }
            }
        ]
        """);

        var invalidArrayValues = JsonDocument.Parse("""
        {
            "skills": [
                { "damage": 50 },
                { "damage": null }
            ]
        }
        """);

        // Act
        var result = engine.ValidateValues(parentSchema, invalidArrayValues, preloaded);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("[1].damage is required"));
    }
}
