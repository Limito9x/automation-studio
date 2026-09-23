using Automation.Pipeline.Features.Pipelines.Dtos;

namespace Automation.Pipeline.Features.Pipelines.GetPipelineExecutions;

public class GetPipelineExecutionsEndpoint(IMessageBus bus) : EndpointWithoutRequest<List<PipelineExecutionDto>>
{
    public override void Configure()
    {
        Get("{pipelineId:guid}/executions");
        Group<PipelinesGroup>();
        Permissions(P.Pipeline.GetById);

        Description(d => d
            .Produces<List<PipelineExecutionDto>>(200)
            .Produces(404));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var pipelineId = Route<Guid>("pipelineId");
        var result = await bus.InvokeAsync<Result<List<PipelineExecutionDto>>>(new GetPipelineExecutionsQuery(pipelineId), ct);
        await this.SendResultAsync(result, ct);
    }
}
