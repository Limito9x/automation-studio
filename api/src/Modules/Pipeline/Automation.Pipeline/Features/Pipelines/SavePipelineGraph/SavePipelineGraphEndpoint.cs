using Automation.Pipeline.Features.Pipelines.Dtos;

namespace Automation.Pipeline.Features.Pipelines.SavePipelineGraph;

public class SavePipelineGraphRequest
{
    public List<SavePipelineNodeItem> Nodes { get; set; } = [];
    public List<SavePipelineEdgeItem> Edges { get; set; } = [];
}

public class SavePipelineGraphEndpoint(IMessageBus bus) : Endpoint<SavePipelineGraphRequest, PipelineGraphDto>
{
    public override void Configure()
    {
        Put("{id:guid}/graph");
        Group<PipelinesGroup>();
        Permissions(P.Pipeline.Update);
        Description(d => d
            .Produces<PipelineGraphDto>(200)
            .Produces(400)
            .Produces(404));
    }

    public override async Task HandleAsync(SavePipelineGraphRequest req, CancellationToken ct)
    {
        var pipelineId = Route<Guid>("id");
        var cmd = new SavePipelineGraphCommand(pipelineId, req.Nodes, req.Edges);
        var result = await bus.InvokeAsync<Result<PipelineGraphDto>>(cmd, ct);
        await this.SendResultAsync(result, ct);
    }
}
