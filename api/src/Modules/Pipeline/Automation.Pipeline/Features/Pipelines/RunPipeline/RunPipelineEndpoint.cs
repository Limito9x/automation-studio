using Automation.Pipeline.Engine.Models;
using Automation.Pipeline.Features.Pipelines.Dtos;

namespace Automation.Pipeline.Features.Pipelines.RunPipeline;

public class RunPipelineEndpoint(IMessageBus bus) : Endpoint<RunPipelineRequest, PipelineExecutionDto>
{
    public override void Configure()
    {
        Post("{pipelineId:guid}/run");
        Group<PipelinesGroup>();
        Permissions(P.Pipeline.Update);
        Description(d => d
            .Produces<PipelineExecutionDto>(200)
            .Produces(400)
            .Produces(422)
            .Produces(404));
    }

    public override async Task HandleAsync(RunPipelineRequest req, CancellationToken ct)
    {
        var pipelineId = Route<Guid>("pipelineId");
        var cmd = new RunPipelineCommand(pipelineId, req.AgentId, req.RuntimeInputs);
        var result = await bus.InvokeAsync<Result<PipelineExecutionDto>>(cmd, ct);

        if (result.IsFailed)
        {
            var unresolvedError = result.Errors.OfType<UnresolvedPinsError>().FirstOrDefault();
            if (unresolvedError != null)
            {
                HttpContext.Response.StatusCode = 422;
                await HttpResponseJsonExtensions.WriteAsJsonAsync(
                    HttpContext.Response,
                    new
                    {
                        error = "UNRESOLVED_PINS",
                        message = unresolvedError.Message,
                        unresolvedPins = unresolvedError.UnresolvedPins
                    },
                    cancellationToken: ct
                );
                return;
            }
        }

        await this.SendResultAsync(result, ct);
    }

}
