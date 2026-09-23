using Automation.Pipeline.Features.Pipelines.Dtos;

namespace Automation.Pipeline.Features.Pipelines.ValidatePipeline;

public class ValidatePipelineEndpoint(IMessageBus bus) : Endpoint<ValidatePipelineQuery, ValidatePipelineResponse>
{
    public override void Configure()
    {
        Post("{pipelineId:guid}/validate");
        Group<PipelinesGroup>();
        Permissions(P.Pipeline.GetById);

        Description(d => d
            .Produces<ValidatePipelineResponse>(200)
            .Produces(400)
            .Produces(404));
    }

    public override async Task HandleAsync(ValidatePipelineQuery req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<ValidatePipelineResponse>>(req, ct);
        await this.SendResultAsync(result, ct);
    }

}
