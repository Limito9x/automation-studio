namespace Automation.Pipeline.Features.Pipelines.DeletePipeline;

public record DeletePipelineRequest(Guid Id);

public class DeletePipelineEndpoint : Endpoint<DeletePipelineRequest>
{
    public override void Configure()
    {
        Delete("{Id:guid}");
        Group<PipelinesGroup>();
        Permissions(P.Pipeline.Delete);
    }

    public override async Task HandleAsync(DeletePipelineRequest req, CancellationToken ct)
    {
        var command = new DeletePipelineCommand(req.Id);
        var result = await Resolve<IMessageBus>().InvokeAsync<Result>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}
