using System.Text.Json;
using Automation.Pipeline.Domain.Entities;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;
using Automation.Pipeline.Engine.Models;

namespace Automation.Pipeline.Features.Pipelines.Dtos;

public record ValidatePipelineQuery(
    Guid PipelineId,
    Dictionary<string, object?>? RuntimeInputs = null
);

public record ValidatePipelineResponse(
    bool IsValid,
    IReadOnlyList<string> CycleNodeIds,
    IReadOnlyList<UnresolvedPin> UnresolvedPins
);

public record RunPipelineRequest(
    Guid AgentId,
    Dictionary<string, object?>? RuntimeInputs = null
);

public record RunPipelineCommand(
    Guid PipelineId,
    Guid AgentId,
    Dictionary<string, object?>? RuntimeInputs = null
);

public record PipelineExecutionDto(
    Guid Id,
    Guid PipelineId,
    Guid AgentId,
    ExecutionStatus Status,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt,
    string? ErrorMessage,
    int NextNodeIndex,
    string? CurrentBatchId,
    JsonDocument? ExecutionState
);

public record NodeExecutionDto(
    Guid Id,
    Guid PipelineExecutionId,
    Guid PipelineNodeId,
    ExecutionStatus Status,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt,
    string? ErrorMessage,
    JsonDocument? Output,
    JsonDocument? Log
);

public record PipelineSummaryDto(
    Guid Id,
    Guid ProjectId,
    string Name,
    PipelineTriggerType TriggerType,
    Guid? TriggerWorkspaceId,
    int NodeCount,
    int EdgeCount,
    DateTimeOffset CreatedAt,
    System.Text.Json.JsonDocument? TriggerConfig = null
);

public record PipelineInputDto(
    Guid Id,
    string Key,
    string Label,
    PinPrimitiveType Type,
    PinCardinality Cardinality,
    bool IsRequired,
    string? DefaultValue,
    int Order
);

public record PipelineOutputDto(
    Guid Id,
    string Key,
    string Label,
    PinPrimitiveType Type,
    PinCardinality Cardinality,
    int Order
);

public record PipelineNodeGraphDto(
    Guid Id,
    string RefId,
    string Kind,
    string Label,
    string? Category,
    string? Executor,
    NodePosition Position,
    IReadOnlyList<PinDefinition> Inputs,
    IReadOnlyList<PinDefinition> Outputs,
    Dictionary<string, object?>? ConfigValues
);

public record PipelineEdgeGraphDto(
    Guid Id,
    Guid SourceNodeId,
    string SourcePin,
    Guid TargetNodeId,
    string TargetPin,
    EdgeKind Kind = EdgeKind.Data
);

public record PipelineVariableDto(
    string Name,
    PinPrimitiveType Type,
    PinCardinality Cardinality = PinCardinality.Single,
    string? Description = null,
    string? StructType = null
);

public record UpdatePipelineVariablesRequest(
    List<PipelineVariableDto> Variables
);

public record UpdatePipelineVariablesCommand(
    Guid PipelineId,
    List<PipelineVariableDto> Variables
);

public record UpdatePipelineTriggerRequest(
    PipelineTriggerType TriggerType,
    Guid? TriggerWorkspaceId,
    System.Text.Json.JsonDocument? TriggerConfig = null
);

public record UpdatePipelineTriggerCommand(
    Guid PipelineId,
    PipelineTriggerType TriggerType,
    Guid? TriggerWorkspaceId,
    System.Text.Json.JsonDocument? TriggerConfig = null
);

public record PipelineGraphDto(
    Guid Id,
    Guid ProjectId,
    string Name,
    PipelineTriggerType TriggerType,
    Guid? TriggerWorkspaceId,
    IReadOnlyList<PipelineNodeGraphDto> Nodes,
    IReadOnlyList<PipelineEdgeGraphDto> Edges,
    IReadOnlyList<PipelineInputDto> Inputs,
    IReadOnlyList<PipelineOutputDto> Outputs,
    IReadOnlyList<PipelineVariableDto> Variables,
    System.Text.Json.JsonDocument? TriggerConfig = null
);

public record SavePipelineNodeItem(
    Guid? Id,
    string RefId,
    string Kind,
    float PositionX,
    float PositionY,
    Dictionary<string, object?>? ConfigValues
);

public record SavePipelineEdgeItem(
    Guid? Id,
    Guid SourceNodeId,
    string SourcePin,
    Guid TargetNodeId,
    string TargetPin
);

public record SavePipelineGraphCommand(
    Guid PipelineId,
    List<SavePipelineNodeItem> Nodes,
    List<SavePipelineEdgeItem> Edges
);
