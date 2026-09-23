using Automation.Pipeline.Features.Pipelines.Dtos;

namespace Automation.Pipeline.Features.Pipelines.AddPipelineEdge;

public record AddPipelineEdgeRequest(
    Guid PipelineId,
    Guid SourcePipelineNodeId,
    string SourcePin,
    Guid TargetPipelineNodeId,
    string TargetPin
);

public class AddPipelineEdgeEndpoint : Endpoint<AddPipelineEdgeRequest, PipelineEdgeGraphDto>
{
    public override void Configure()
    {
        Post("{PipelineId:guid}/edges");
        Group<PipelinesGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(AddPipelineEdgeRequest req, CancellationToken ct)
    {
        var command = req.Adapt<AddPipelineEdgeCommand>();
        var result = await Resolve<IMessageBus>().InvokeAsync<Result<PipelineEdgeGraphDto>>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}
