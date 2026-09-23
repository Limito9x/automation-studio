using Automation.Pipeline.Features.Pipelines.Dtos;

namespace Automation.Pipeline.Features.Pipelines.CreatePipeline;

public class CreatePipelineEndpoint(IMessageBus bus) : Endpoint<CreatePipelineCommand, PipelineSummaryDto>
{
    public override void Configure()
    {
        Post("");
        Group<PipelinesGroup>();
        Permissions(P.Pipeline.Create);
        Description(d => d
            .Produces<PipelineSummaryDto>(200)
            .Produces(400));
    }

    public override async Task HandleAsync(CreatePipelineCommand req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<PipelineSummaryDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
