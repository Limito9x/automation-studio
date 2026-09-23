using Automation.Pipeline.Features.Pipelines.Dtos;

namespace Automation.Pipeline.Features.Pipelines.GetNodeExecutions;

public class GetNodeExecutionsEndpoint(IMessageBus bus) : EndpointWithoutRequest<List<NodeExecutionDto>>
{
    public override void Configure()
    {
        Get("executions/{id:guid}/node-executions");
        Group<PipelinesGroup>();
        Permissions(P.Pipeline.GetById);

        Description(d => d
            .Produces<List<NodeExecutionDto>>(200)
            .Produces(404));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await bus.InvokeAsync<Result<List<NodeExecutionDto>>>(new GetNodeExecutionsQuery(id), ct);
        await this.SendResultAsync(result, ct);
    }
}
