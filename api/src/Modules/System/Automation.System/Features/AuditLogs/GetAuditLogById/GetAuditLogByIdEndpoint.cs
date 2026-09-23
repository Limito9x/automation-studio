using Automation.SystemModule.Shared.Dtos;

namespace Automation.SystemModule.Features.AuditLogs.GetAuditLogById;

public class GetAuditLogByIdEndpoint(IMessageBus bus)
    : Endpoint<GetAuditLogByIdQuery, AuditLogDto>
{
    public override void Configure()
    {
        Get("/{id}");
        Group<AuditLogsGroup>();
        Permissions(P.AuditLogs.GetById);
        Description(x => x.WithName("GetAuditLogById"));
    }

    public override async Task HandleAsync(
        GetAuditLogByIdQuery req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<AuditLogDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}



