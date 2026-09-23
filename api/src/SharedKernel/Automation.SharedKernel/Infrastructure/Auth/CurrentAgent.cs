using Automation.SharedKernel.Abstractions.Auth;
using Microsoft.AspNetCore.Http;

namespace Automation.SharedKernel.Infrastructure.Auth;

public class CurrentAgent(IHttpContextAccessor httpContextAccessor) : ICurrentAgent
{
    public const string HttpContextItemKey = "CurrentAgentId";

    public Guid? AgentId => httpContextAccessor.HttpContext?.Items[HttpContextItemKey] as Guid?;

    public bool IsAgentRequest => AgentId.HasValue;
}

