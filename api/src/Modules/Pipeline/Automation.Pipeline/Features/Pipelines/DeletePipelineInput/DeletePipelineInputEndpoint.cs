namespace Automation.Pipeline.Features.Pipelines.DeletePipelineInput;

public record DeletePipelineInputRequest(
    Guid PipelineId,
    Guid InputId
);

public class DeletePipelineInputEndpoint : Endpoint<DeletePipelineInputRequest>
{
    public override void Configure()
    {
        Delete("{PipelineId:guid}/inputs/{InputId:guid}");
        Group<PipelinesGroup>();
        Permissions(P.Pipeline.Update);
    }

    public override async Task HandleAsync(DeletePipelineInputRequest req, CancellationToken ct)
    {
        var command = req.Adapt<DeletePipelineInputCommand>();
        var result = await Resolve<IMessageBus>().InvokeAsync<Result>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}
