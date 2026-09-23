using Automation.Pipeline.Engine.Parsers;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Nodes.ParseScript;

[NonTransactional]
public class ParseScriptHandler
{
    public Task<Result<ParseScriptResponseDto>> HandleAsync(
        ParseScriptCommand command,
        CancellationToken ct
    )
    {
        if (string.IsNullOrWhiteSpace(command.ScriptContent))
        {
            return Task.FromResult(Result.Fail<ParseScriptResponseDto>("Script content is empty."));
        }

        var parsed = PythonScriptSchemaParser.Parse(command.ScriptContent, command.FileName);

        return Task.FromResult(Result.Ok(new ParseScriptResponseDto(
            parsed.SuggestedName,
            parsed.SuggestedLabel,
            parsed.Executor,
            parsed.Description,
            parsed.Inputs,
            parsed.Outputs
        )));
    }
}
