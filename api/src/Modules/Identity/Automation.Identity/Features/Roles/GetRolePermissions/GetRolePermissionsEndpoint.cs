using Microsoft.AspNetCore.Http;
using FastEndpoints;
using Wolverine;

namespace Automation.Identity.Features.Roles.GetRolePermissions;

public class GetRolePermissionsEndpoint(IMessageBus bus)
    : Endpoint<GetRolePermissionsQuery, List<string>>
{
    public override void Configure()
    {
        Get("/{id}/permissions");
        Group<RolesGroup>();
        Permissions(P.RolesFeature.Assign);
        
    }

    public override async Task HandleAsync(
        GetRolePermissionsQuery req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<List<string>>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}




