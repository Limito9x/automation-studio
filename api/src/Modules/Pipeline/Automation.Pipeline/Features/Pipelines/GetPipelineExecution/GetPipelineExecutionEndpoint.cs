using Automation.Pipeline.Features.Pipelines.Dtos;

namespace Automation.Pipeline.Features.Pipelines.GetPipelineExecution;

public class GetPipelineExecutionEndpoint(IMessageBus bus) : EndpointWithoutRequest<PipelineExecutionDto>
{
    public override void Configure()
    {
        Get("executions/{id:guid}");
        Group<PipelinesGroup>();
        Permissions(P.Pipeline.GetById);

        Description(d => d
            .Produces<PipelineExecutionDto>(200)
            .Produces(404));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await bus.InvokeAsync<Result<PipelineExecutionDto>>(new GetPipelineExecutionQuery(id), ct);
        await this.SendResultAsync(result, ct);
    }

}
