using Automation.SharedKernel.Extensions.Results;
using FastEndpoints;
using Wolverine;

namespace Automation.Identity.Features.Users.BulkUpdateStatus;

public class BulkUpdateUserStatusEndpoint(IMessageBus bus) : Endpoint<BulkUpdateUserStatusCommand, string>
{
    public override void Configure()
    {
        Put("bulk-status");
        Group<UsersGroup>();
    }

    public override async Task HandleAsync(BulkUpdateUserStatusCommand req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<FluentResults.Result<string>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}



