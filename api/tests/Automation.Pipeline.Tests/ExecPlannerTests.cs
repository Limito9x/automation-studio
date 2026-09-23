using Automation.Pipeline.Domain.Entities;
using PipelineEntity = Automation.Pipeline.Domain.Entities.Pipeline;
using Automation.Pipeline.Domain.ValueObjects;
using Automation.Pipeline.Engine.ExecPlanner;
using Automation.Pipeline.Tools;
using FluentAssertions;
using Xunit;

namespace Automation.Pipeline.Tests;

public class ExecPlannerTests
{
    private readonly ExecPlanner _planner = new();
    private readonly FakeToolRegistry _toolRegistry = new();

    [Fact]
    public void BuildExecPlan_ContiguousAgentSteps_ShouldFuseIntoSingleSegment()
    {
        // Setup Pipeline: Start (dotNet) -> Import (blender) -> Save (blender) -> Export (blender)
        var pipeline = CreatePipeline("TestPipeline");

        var startNode = CreateNode(pipeline.Id, "Start", "Start");
        var importNode = CreateNode(pipeline.Id, "custom_import", "Tool");
        var saveNode = CreateNode(pipeline.Id, "custom_save", "Tool");
        var exportNode = CreateNode(pipeline.Id, "custom_export", "Tool");

        // Pure Node connected only via Data wire (BreakStruct)
        var breakStructNode = CreateNode(pipeline.Id, "BreakStruct", "Tool");

        pipeline.AddNode(startNode);
        pipeline.AddNode(importNode);
        pipeline.AddNode(saveNode);
        pipeline.AddNode(exportNode);
        pipeline.AddNode(breakStructNode);

        // Exec Wires: Start -> Import -> Save -> Export
        pipeline.AddEdge(startNode.Id, "exec_out", importNode.Id, "exec_in");
        pipeline.AddEdge(importNode.Id, "exec_out", saveNode.Id, "exec_in");
        pipeline.AddEdge(saveNode.Id, "exec_out", exportNode.Id, "exec_in");

        // Data Wire: BreakStruct -> Import (Data wire only)
        pipeline.AddEdge(breakStructNode.Id, "Path", importNode.Id, "FilePath");

        var customDefs = new List<NodeDefinition>
        {
            new(pipeline.ProjectId, "Import Mesh", "custom_import", "Import Mesh", "blender", [], []),
            new(pipeline.ProjectId, "Save File", "custom_save", "Save File", "blender", [], []),
            new(pipeline.ProjectId, "Export FBX", "custom_export", "Export FBX", "blender", [], []),
        };

        var plan = _planner.BuildExecPlan(pipeline, customDefs, _toolRegistry);

        plan.IsValid.Should().BeTrue();
        plan.Segments.Should().HaveCount(2);

        // Segment 1: dotNet (Start)
        plan.Segments[0].Executor.Should().Be("dotNet");
        plan.Segments[0].Steps.Should().HaveCount(1);
        plan.Segments[0].Steps[0].NodeId.Should().Be(startNode.Id);

        // Segment 2: blender (Import + Save + Export fused together!)
        plan.Segments[1].Executor.Should().Be("blender");
        plan.Segments[1].Steps.Should().HaveCount(3);
        plan.Segments[1].Steps[0].NodeId.Should().Be(importNode.Id);
        plan.Segments[1].Steps[1].NodeId.Should().Be(saveNode.Id);
        plan.Segments[1].Steps[2].NodeId.Should().Be(exportNode.Id);

        // BreakStruct (Pure node) must NOT be present in ExecPlan segments!
        plan.GetAllSteps().Any(s => s.NodeId == breakStructNode.Id).Should().BeFalse();
    }

    [Fact]
    public void BuildExecPlan_WithForEachLoop_ShouldConstructBodyPlanAndContinuation()
    {
        // Setup: Start (dotNet) -> ForEach (dotNet, FlowControl)
        //   ForEach -> loop_body -> ProcessStep (blender)
        //   ForEach -> completed -> NotifyStep (dotNet)
        var pipeline = CreatePipeline("ForEachPipeline");

        var startNode = CreateNode(pipeline.Id, "Start", "Start");
        var forEachNode = CreateNode(pipeline.Id, "ForEach", "FlowControl");
        var processNode = CreateNode(pipeline.Id, "custom_process", "Tool");
        var notifyNode = CreateNode(pipeline.Id, "custom_notify", "Tool");

        pipeline.AddNode(startNode);
        pipeline.AddNode(forEachNode);
        pipeline.AddNode(processNode);
        pipeline.AddNode(notifyNode);

        pipeline.AddEdge(startNode.Id, "exec_out", forEachNode.Id, "exec_in");
        pipeline.AddEdge(forEachNode.Id, "loop_body", processNode.Id, "exec_in");
        pipeline.AddEdge(forEachNode.Id, "completed", notifyNode.Id, "exec_in");

        var customDefs = new List<NodeDefinition>
        {
            new(pipeline.ProjectId, "Process", "custom_process", "Process", "blender", [], []),
            new(pipeline.ProjectId, "Notify", "custom_notify", "Notify", "dotNet", [], []),
        };

        var plan = _planner.BuildExecPlan(pipeline, customDefs, _toolRegistry);

        plan.IsValid.Should().BeTrue();
        plan.Segments.Should().HaveCount(3);

        // Segment 1: Start (dotNet)
        plan.Segments[0].Executor.Should().Be("dotNet");
        plan.Segments[0].Steps[0].NodeId.Should().Be(startNode.Id);

        // Segment 2: ForEach (FlowControl)
        var fcSegment = plan.Segments[1];
        fcSegment.IsFlowControl.Should().BeTrue();
        fcSegment.Steps[0].NodeId.Should().Be(forEachNode.Id);
        fcSegment.BodyPlan.Should().NotBeNull();
        fcSegment.BodyPlan!.Segments.Should().HaveCount(1);
        fcSegment.BodyPlan.Segments[0].Executor.Should().Be("blender");
        fcSegment.BodyPlan.Segments[0].Steps[0].NodeId.Should().Be(processNode.Id);

        // Segment 3: Continuation (Notify)
        plan.Segments[2].Executor.Should().Be("dotNet");
        plan.Segments[2].Steps[0].NodeId.Should().Be(notifyNode.Id);
    }

    [Fact]
    public void BuildExecPlan_WhenCycleExists_ShouldDetectCycle()
    {
        var pipeline = CreatePipeline("CyclePipeline");

        var startNode = CreateNode(pipeline.Id, "Start", "Start");
        var nodeA = CreateNode(pipeline.Id, "custom_a", "Tool");
        var nodeB = CreateNode(pipeline.Id, "custom_b", "Tool");

        pipeline.AddNode(startNode);
        pipeline.AddNode(nodeA);
        pipeline.AddNode(nodeB);

        pipeline.AddEdge(startNode.Id, "exec_out", nodeA.Id, "exec_in");
        pipeline.AddEdge(nodeA.Id, "exec_out", nodeB.Id, "exec_in");
        pipeline.AddEdge(nodeB.Id, "exec_out", nodeA.Id, "exec_in"); // Cycle!

        var customDefs = new List<NodeDefinition>
        {
            new(pipeline.ProjectId, "A", "custom_a", "A", "blender", [], []),
            new(pipeline.ProjectId, "B", "custom_b", "B", "blender", [], [])
        };

        var plan = _planner.BuildExecPlan(pipeline, customDefs, _toolRegistry);

        plan.IsValid.Should().BeFalse();
        plan.CycleNodeIds.Should().NotBeEmpty();
    }

    // Helpers
    private static PipelineEntity CreatePipeline(string name)
    {
        return new PipelineEntity(Guid.NewGuid(), name);
    }

    private static PipelineNode CreateNode(Guid pipelineId, string refId, string kind)
    {
        return new PipelineNode(Guid.NewGuid(), pipelineId, refId, kind, 0, 0);
    }

    private class FakeToolRegistry : IToolRegistry
    {
        private readonly List<IResolverTool> _tools =
        [
            new FakeTool("BreakStruct", "Break Struct", isPure: true),
            new FakeTool("ForEach", "For Each", isPure: false, category: "Flow Control"),
        ];

        public IReadOnlyList<IResolverTool> GetAll() => _tools;
        public IResolverTool? GetByKey(string key) => _tools.FirstOrDefault(t => string.Equals(t.Key, key, StringComparison.OrdinalIgnoreCase));
    }

    private class FakeTool(string key, string label, bool isPure, string? category = null) : IResolverTool
    {
        public string Key => key;
        public string Label => label;
        public string? Category => category;
        public bool IsPure => isPure;
        public IReadOnlyList<PinDefinition> Inputs => [];
        public IReadOnlyList<PinDefinition> Outputs => [];
        public Task<Dictionary<string, object>> ExecuteAsync(Dictionary<string, object> inputs, ToolExecutionContext context)
            => Task.FromResult(new Dictionary<string, object>());
    }
}
