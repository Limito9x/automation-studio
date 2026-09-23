namespace Automation.Agent.Features.Agents.GenerateSetupToken;

public record GenerateSetupTokenCommand();

public record SetupTokenDto(string Token, DateTimeOffset ExpiresAt);
