using Automation.Pipeline.Features.Pipelines.Dtos;

namespace Automation.Pipeline.Features.Pipelines.UpdatePipeline;

public class UpdatePipelineEndpoint(IMessageBus bus) : Endpoint<UpdatePipelineRequest, PipelineSummaryDto>
{
    public override void Configure()
    {
        Patch("{id:guid}");
        Group<PipelinesGroup>();
        Permissions(P.Pipeline.Update);
        Description(d => d
            .Produces<PipelineSummaryDto>(200)
            .Produces(400)
            .Produces(404));
    }

    public override async Task HandleAsync(UpdatePipelineRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await bus.InvokeAsync<Result<PipelineSummaryDto>>(new UpdatePipelineCommand(id, req.Name), ct);
        await this.SendResultAsync(result, ct);
    }
}
