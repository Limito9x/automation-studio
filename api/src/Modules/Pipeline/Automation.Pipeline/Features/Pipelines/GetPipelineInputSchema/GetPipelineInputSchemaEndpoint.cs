using Automation.Pipeline.Features.Pipelines.Dtos;

namespace Automation.Pipeline.Features.Pipelines.GetPipelineInputSchema;

public class GetPipelineInputSchemaEndpoint(IMessageBus bus)
    : EndpointWithoutRequest<IReadOnlyList<PipelineInputDto>>
{
    public override void Configure()
    {
        Get("{pipelineId:guid}/input-schema");
        Group<PipelinesGroup>();
        Permissions(P.Pipeline.GetById);
        Description(d => d
            .Produces<IReadOnlyList<PipelineInputDto>>(200)
            .Produces(404));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var pipelineId = Route<Guid>("pipelineId");
        var query = new GetPipelineInputSchemaQuery(pipelineId);
        var result = await bus.InvokeAsync<Result<IReadOnlyList<PipelineInputDto>>>(query, ct);
        await this.SendResultAsync(result, ct);
    }
}
