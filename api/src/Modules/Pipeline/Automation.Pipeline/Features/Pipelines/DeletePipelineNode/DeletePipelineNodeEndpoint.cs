namespace Automation.Pipeline.Features.Pipelines.DeletePipelineNode;

public record DeletePipelineNodeRequest(
    Guid PipelineId,
    Guid NodeId
);

public class DeletePipelineNodeEndpoint : Endpoint<DeletePipelineNodeRequest>
{
    public override void Configure()
    {
        Delete("{PipelineId:guid}/nodes/{NodeId:guid}");
        Group<PipelinesGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(DeletePipelineNodeRequest req, CancellationToken ct)
    {
        var command = req.Adapt<DeletePipelineNodeCommand>();
        var result = await Resolve<IMessageBus>().InvokeAsync<Result>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}
