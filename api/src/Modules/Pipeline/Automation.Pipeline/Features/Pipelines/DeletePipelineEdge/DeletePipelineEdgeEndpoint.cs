namespace Automation.Pipeline.Features.Pipelines.DeletePipelineEdge;

public record DeletePipelineEdgeRequest(
    Guid PipelineId,
    Guid EdgeId
);

public class DeletePipelineEdgeEndpoint : Endpoint<DeletePipelineEdgeRequest>
{
    public override void Configure()
    {
        Delete("{PipelineId:guid}/edges/{EdgeId:guid}");
        Group<PipelinesGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(DeletePipelineEdgeRequest req, CancellationToken ct)
    {
        var command = req.Adapt<DeletePipelineEdgeCommand>();
        var result = await Resolve<IMessageBus>().InvokeAsync<Result>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}
