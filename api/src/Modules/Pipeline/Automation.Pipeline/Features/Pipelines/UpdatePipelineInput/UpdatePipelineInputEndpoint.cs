using Automation.Pipeline.Features.Pipelines.Dtos;

namespace Automation.Pipeline.Features.Pipelines.UpdatePipelineInput;

public record UpdatePipelineInputRequest(
    Guid PipelineId,
    Guid InputId,
    string? Key = null,
    string? Label = null,
    string? Type = null,
    string? Cardinality = null,
    bool? IsRequired = null,
    string? DefaultValue = null,
    int? Order = null
);

public class UpdatePipelineInputEndpoint : Endpoint<UpdatePipelineInputRequest, PipelineInputDto>
{
    public override void Configure()
    {
        Patch("{PipelineId:guid}/inputs/{InputId:guid}");
        Group<PipelinesGroup>();
        Permissions(P.Pipeline.Update);
    }

    public override async Task HandleAsync(UpdatePipelineInputRequest req, CancellationToken ct)
    {
        var command = req.Adapt<UpdatePipelineInputCommand>();
        var result = await Resolve<IMessageBus>().InvokeAsync<Result<PipelineInputDto>>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}
