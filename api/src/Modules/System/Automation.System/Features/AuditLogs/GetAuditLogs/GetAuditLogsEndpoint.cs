using Automation.SystemModule.Shared.Dtos;

namespace Automation.SystemModule.Features.AuditLogs.GetAuditLogs;

public class GetAuditLogsEndpoint(IMessageBus bus)
    : Endpoint<GetAuditLogsQuery, PagedResult<AuditLogDto>>
{
    public override void Configure()
    {
        Get("/");
        Group<AuditLogsGroup>();
        Permissions(P.AuditLogs.GetAll);
        Description(x => x.WithName("GetAuditLogs"));
        RequestBinder(new PagedQueryBinder<GetAuditLogsQuery>());
    }

    public override async Task HandleAsync(
        GetAuditLogsQuery req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<PagedResult<AuditLogDto>>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}



