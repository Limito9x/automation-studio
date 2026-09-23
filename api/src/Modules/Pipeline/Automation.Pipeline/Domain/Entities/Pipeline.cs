using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;

namespace Automation.Pipeline.Domain.Entities;

public class Pipeline : BaseEntity
{
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public PipelineTriggerType TriggerType { get; set; } = PipelineTriggerType.Manual;
    public Guid? TriggerWorkspaceId { get; set; }
    public System.Text.Json.JsonDocument? TriggerConfig { get; set; }
    public List<PipelineVariableDecl> Variables { get; set; } = new();
    private readonly List<PipelineNode> _nodes = new();
    public IReadOnlyList<PipelineNode> Nodes => _nodes;
    private readonly List<PipelineEdge> _edges = new();
    public IReadOnlyList<PipelineEdge> Edges => _edges;
    private readonly List<PipelineInput> _inputs = new();
    public IReadOnlyList<PipelineInput> Inputs => _inputs;
    private readonly List<PipelineOutput> _outputs = new();
    public IReadOnlyList<PipelineOutput> Outputs => _outputs;

    public Pipeline() { }

    public Pipeline(
        Guid projectId,
        string name,
        PipelineTriggerType triggerType = PipelineTriggerType.Manual,
        Guid? triggerWorkspaceId = null,
        System.Text.Json.JsonDocument? triggerConfig = null
    )
    {
        ProjectId = projectId;
        Name = name;
        TriggerType = triggerType;
        TriggerWorkspaceId = triggerWorkspaceId;
        TriggerConfig = triggerConfig;
    }

    public void UpdateTrigger(PipelineTriggerType triggerType, Guid? triggerWorkspaceId = null, System.Text.Json.JsonDocument? triggerConfig = null)
    {
        TriggerType = triggerType;
        TriggerWorkspaceId = triggerWorkspaceId;
        TriggerConfig = triggerConfig;
    }

    public void UpdateName(string name)
    {
        Name = name;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AddInput(PipelineInput input)
    {
        _inputs.Add(input);
    }

    public void RemoveInput(Guid inputId)
    {
        var input = _inputs.FirstOrDefault(x => x.Id == inputId);
        if (input != null)
        {
            _inputs.Remove(input);
        }
    }

    public void AddOutput(PipelineOutput output)
    {
        _outputs.Add(output);
    }

    public void RemoveOutput(Guid outputId)
    {
        var output = _outputs.FirstOrDefault(x => x.Id == outputId);
        if (output != null)
        {
            _outputs.Remove(output);
        }
    }

    public void AddNode(string refId, string kind, float x, float y, System.Text.Json.JsonDocument? config = null)
    {
        _nodes.Add(new PipelineNode(IdGenerator.NewId(), Id, refId, kind, x, y, config));
    }

    public void AddNode(PipelineNode node)
    {
        _nodes.Add(node);
    }

    public void RemoveNode(Guid nodeId)
    {
        var node = _nodes.FirstOrDefault(x => x.Id == nodeId);
        if (node != null)
        {
            _nodes.Remove(node);
        }
    }

    public void UpdateNode(Guid nodeId, float x, float y)
    {
        var node = _nodes.FirstOrDefault(x => x.Id == nodeId);
        node?.Update(x, y);
    }

    public void UpdateNodeConfig(Guid nodeId, System.Text.Json.JsonDocument? config)
    {
        var node = _nodes.FirstOrDefault(x => x.Id == nodeId);
        node?.UpdateConfig(config);
    }

    public void AddEdge(
        Guid sourcePipelineNodeId,
        string sourcePin,
        Guid targetPipelineNodeId,
        string targetPin
    )
    {
        _edges.Add(
            new PipelineEdge(Id, sourcePipelineNodeId, sourcePin, targetPipelineNodeId, targetPin)
        );
    }

    public void RemoveEdge(Guid edgeId)
    {
        var edge = _edges.FirstOrDefault(x => x.Id == edgeId);
        if (edge != null)
        {
            _edges.Remove(edge);
        }
    }

    public void ClearEdges()
    {
        _edges.Clear();
    }

    public void SetVariables(List<PipelineVariableDecl> variables)
    {
        Variables = variables ?? new List<PipelineVariableDecl>();
    }
}
