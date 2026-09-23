namespace Automation.Agent.Contracts;

public record ExecutorCandidateDto(
    string ExecutorKey,
    string ExecutablePath,
    string Version
);
