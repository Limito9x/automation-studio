namespace Automation.Pipeline.Features.Pipelines.UpdatePipelineNode;

public record UpdatePipelineNodeRequest(
    Guid PipelineId,
    Guid NodeId,
    float? PositionX = null,
    float? PositionY = null,
    Dictionary<string, object?>? ConfigValues = null
);

public class UpdatePipelineNodeEndpoint : Endpoint<UpdatePipelineNodeRequest>
{
    public override void Configure()
    {
        Patch("{PipelineId:guid}/nodes/{NodeId:guid}");
        Group<PipelinesGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdatePipelineNodeRequest req, CancellationToken ct)
    {
        var command = req.Adapt<UpdatePipelineNodeCommand>();
        var result = await Resolve<IMessageBus>().InvokeAsync<Result>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}
