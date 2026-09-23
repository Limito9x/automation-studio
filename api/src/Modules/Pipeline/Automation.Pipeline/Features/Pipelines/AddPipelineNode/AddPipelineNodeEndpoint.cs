using Automation.Pipeline.Features.Pipelines.Dtos;

namespace Automation.Pipeline.Features.Pipelines.AddPipelineNode;

public record AddPipelineNodeRequest(
    Guid PipelineId,
    string RefId,
    string Kind,
    float PositionX,
    float PositionY,
    Dictionary<string, object?>? ConfigValues = null
);

public class AddPipelineNodeEndpoint : Endpoint<AddPipelineNodeRequest, PipelineNodeGraphDto>
{
    public override void Configure()
    {
        Post("{PipelineId:guid}/nodes");
        Group<PipelinesGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(AddPipelineNodeRequest req, CancellationToken ct)
    {
        var command = req.Adapt<AddPipelineNodeCommand>();
        var result = await Resolve<IMessageBus>().InvokeAsync<Result<PipelineNodeGraphDto>>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}
