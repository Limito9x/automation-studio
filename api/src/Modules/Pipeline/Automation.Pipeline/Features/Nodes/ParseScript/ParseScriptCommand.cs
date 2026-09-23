namespace Automation.Pipeline.Features.Nodes.ParseScript;

public record ParseScriptCommand(
    string? ScriptContent,
    string? FileName = null
);
