using Automation.Pipeline.Features.Pipelines.Dtos;

namespace Automation.Pipeline.Features.Pipelines.AddPipelineInput;

public record AddPipelineInputRequest(
    Guid PipelineId,
    string Key,
    string Label,
    string Type,
    string Cardinality = "Single",
    bool IsRequired = true,
    string? DefaultValue = null,
    int Order = 0
);

public class AddPipelineInputEndpoint : Endpoint<AddPipelineInputRequest, PipelineInputDto>
{
    public override void Configure()
    {
        Post("{PipelineId:guid}/inputs");
        Group<PipelinesGroup>();
        Permissions(P.Pipeline.Update);
    }

    public override async Task HandleAsync(AddPipelineInputRequest req, CancellationToken ct)
    {
        var command = req.Adapt<AddPipelineInputCommand>();
        var result = await Resolve<IMessageBus>().InvokeAsync<Result<PipelineInputDto>>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}
