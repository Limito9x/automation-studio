using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Automation.SharedKernel.Infrastructure.Persistence;

public class CurrentUserProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUserProvider
{
    public Guid? UserId => Guid.TryParse(httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;
}



