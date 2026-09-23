using Automation.Identity.Constants;
using FastEndpoints;
using FluentResults;

using Wolverine;

namespace Automation.Identity.Features.Users.AssignUserRoles;

public class AssignUserRolesEndpoint(IMessageBus bus) : Endpoint<AssignUserRolesCommand, Result>
{
    public override void Configure()
    {
        Post("/{id}/roles");
        Group<UsersGroup>();
        Permissions(IdentityPermissions.Users.Update);
    }

    public override async Task HandleAsync(AssignUserRolesCommand req, CancellationToken ct)
    {
        req.Id = Route<Guid>("id");
        var result = await bus.InvokeAsync<Result>(req, ct);
        
        await this.SendResultAsync(result, ct);
    }
}



