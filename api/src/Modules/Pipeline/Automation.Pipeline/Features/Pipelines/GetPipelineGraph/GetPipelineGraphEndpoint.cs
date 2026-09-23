using Automation.Pipeline.Features.Pipelines.Dtos;

namespace Automation.Pipeline.Features.Pipelines.GetPipelineGraph;

public class GetPipelineGraphEndpoint(IMessageBus bus) : EndpointWithoutRequest<PipelineGraphDto>
{
    public override void Configure()
    {
        Get("{id:guid}/graph");
        Group<PipelinesGroup>();
        Permissions(P.Pipeline.GetById);
        Description(d => d
            .Produces<PipelineGraphDto>(200)
            .Produces(404));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var pipelineId = Route<Guid>("id");
        var query = new GetPipelineGraphQuery(pipelineId);
        var result = await bus.InvokeAsync<Result<PipelineGraphDto>>(query, ct);
        await this.SendResultAsync(result, ct);
    }
}
