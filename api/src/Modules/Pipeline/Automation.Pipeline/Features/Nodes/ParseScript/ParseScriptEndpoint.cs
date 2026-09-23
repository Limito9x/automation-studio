namespace Automation.Pipeline.Features.Nodes.ParseScript;

public class ParseScriptEndpoint(IMessageBus bus)
    : Endpoint<ParseScriptCommand, ParseScriptResponseDto>
{
    public override void Configure()
    {
        Post("parse-script");
        Group<NodesGroup>();
        Description(x => x.WithName("ParseScriptSchema"));
        Permissions(P.Pipeline.GetAll);
    }

    public override async Task HandleAsync(ParseScriptCommand req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<ParseScriptResponseDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
