using System.Text.Json;
using Automation.DynamicForms.Services;
using FluentAssertions;
using Xunit;

namespace Automation.DynamicForms.Tests.Services;

public class StructDependencyHelperTests
{
    [Fact]
    public void ExtractDirectStructIds_ShouldExtractAllValidGuids()
    {
        // Arrange
        var structId1 = Guid.NewGuid();
        var structId2 = Guid.NewGuid();

        var json = $$"""
        [
            {
                "name": "basic_field",
                "type": "text",
                "properties": { "required": true }
            },
            {
                "name": "slot_binding",
                "type": "struct",
                "properties": {
                    "structId": "{{structId1}}",
                    "structName": "SlotBinding",
                    "cardinality": "single"
                }
            },
            {
                "name": "skills",
                "type": "struct",
                "properties": {
                    "structId": "{{structId2}}",
                    "structName": "Skill",
                    "cardinality": "array"
                }
            }
        ]
        """;

        using var doc = JsonDocument.Parse(json);

        // Act
        var result = StructDependencyHelper.ExtractDirectStructIds(doc);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(structId1);
        result.Should().Contain(structId2);
    }

    [Fact]
    public void ExtractDirectStructIds_ShouldReturnEmpty_WhenFieldsNullOrNoStructs()
    {
        // Act
        var resultNull = StructDependencyHelper.ExtractDirectStructIds(null);

        using var docNoStructs = JsonDocument.Parse("""[{"name":"field1","properties":{}}]""");
        var resultNoStructs = StructDependencyHelper.ExtractDirectStructIds(docNoStructs);

        // Assert
        resultNull.Should().BeEmpty();
        resultNoStructs.Should().BeEmpty();
    }

    [Fact]
    public void DetectCycle_ShouldFail_WhenDirectSelfReference()
    {
        // Arrange
        var structA = Guid.NewGuid();
        var directDependencies = new List<Guid> { structA };
        var graph = new Dictionary<Guid, IEnumerable<Guid>>();

        // Act
        var result = StructDependencyHelper.DetectCycle(structA, directDependencies, graph);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Contain("cannot reference itself");
    }

    [Fact]
    public void DetectCycle_ShouldFail_WhenTwoNodeCycle()
    {
        // Arrange: A -> B, and B -> A
        var structA = Guid.NewGuid();
        var structB = Guid.NewGuid();

        var graph = new Dictionary<Guid, IEnumerable<Guid>>
        {
            [structB] = [structA]
        };

        // Act: Struct A tries to add Struct B
        var result = StructDependencyHelper.DetectCycle(structA, [structB], graph);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Contain("Circular dependency detected");
    }

    [Fact]
    public void DetectCycle_ShouldFail_WhenDeepTransitiveCycle()
    {
        // Arrange: A -> B -> C -> D -> A
        var structA = Guid.NewGuid();
        var structB = Guid.NewGuid();
        var structC = Guid.NewGuid();
        var structD = Guid.NewGuid();

        var graph = new Dictionary<Guid, IEnumerable<Guid>>
        {
            [structB] = [structC],
            [structC] = [structD],
            [structD] = [structA]
        };

        // Act
        var result = StructDependencyHelper.DetectCycle(structA, [structB], graph);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Contain("Circular dependency detected");
    }

    [Fact]
    public void ComputeTransitiveDependencies_ShouldReturnFlattenedSet_WhenValidDag()
    {
        // Arrange:
        // A -> B, C
        // B -> D
        // C -> D, E
        // Result should be distinct [B, C, D, E]
        var structA = Guid.NewGuid();
        var structB = Guid.NewGuid();
        var structC = Guid.NewGuid();
        var structD = Guid.NewGuid();
        var structE = Guid.NewGuid();

        var graph = new Dictionary<Guid, IEnumerable<Guid>>
        {
            [structB] = [structD],
            [structC] = [structD, structE],
            [structD] = [],
            [structE] = []
        };

        // Act
        var result = StructDependencyHelper.ComputeTransitiveDependencies(structA, [structB, structC], graph);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(4);
        result.Value.Should().Contain([structB, structC, structD, structE]);
    }

    [Fact]
    public async Task ComputeTransitiveDependenciesAsync_ShouldWorkWithAsyncLookup()
    {
        // Arrange
        var structA = Guid.NewGuid();
        var structB = Guid.NewGuid();
        var structC = Guid.NewGuid();

        var graph = new Dictionary<Guid, List<Guid>>
        {
            [structB] = [structC],
            [structC] = []
        };

        Task<IEnumerable<Guid>?> MockLookup(Guid id, CancellationToken ct)
        {
            graph.TryGetValue(id, out var list);
            return Task.FromResult<IEnumerable<Guid>?>(list);
        }

        // Act
        var result = await StructDependencyHelper.ComputeTransitiveDependenciesAsync(structA, [structB], MockLookup);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain([structB, structC]);
    }
}
