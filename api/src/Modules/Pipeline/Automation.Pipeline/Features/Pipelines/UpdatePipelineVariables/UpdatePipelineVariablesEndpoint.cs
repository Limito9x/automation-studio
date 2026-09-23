using Automation.Pipeline.Features.Pipelines.Dtos;

namespace Automation.Pipeline.Features.Pipelines.UpdatePipelineVariables;

public record UpdatePipelineVariablesRequest(
    Guid PipelineId,
    List<PipelineVariableDto> Variables
);

public class UpdatePipelineVariablesEndpoint : Endpoint<UpdatePipelineVariablesRequest, List<PipelineVariableDto>>
{
    public override void Configure()
    {
        Put("{PipelineId:guid}/variables");
        Group<PipelinesGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdatePipelineVariablesRequest req, CancellationToken ct)
    {
        var command = new UpdatePipelineVariablesCommand(req.PipelineId, req.Variables);
        var result = await Resolve<IMessageBus>().InvokeAsync<Result<List<PipelineVariableDto>>>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}
