using Automation.Pipeline.Features.Pipelines.Dtos;

namespace Automation.Pipeline.Features.Pipelines.GetPipelines;

public class GetPipelinesRequest
{
    public Guid? ProjectId { get; set; }
}

public class GetPipelinesEndpoint(IMessageBus bus) : Endpoint<GetPipelinesRequest, List<PipelineSummaryDto>>
{
    public override void Configure()
    {
        Get("");
        Group<PipelinesGroup>();
        Permissions(P.Pipeline.GetAll);
        Description(d => d
            .Produces<List<PipelineSummaryDto>>(200)
            .Produces(400));
    }

    public override async Task HandleAsync(GetPipelinesRequest req, CancellationToken ct)
    {
        var query = new GetPipelinesQuery(req.ProjectId);
        var result = await bus.InvokeAsync<Result<List<PipelineSummaryDto>>>(query, ct);
        await this.SendResultAsync(result, ct);
    }
}
