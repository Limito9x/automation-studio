using Automation.Identity.Domain.Enums;

namespace Automation.Identity.Features.Users.BulkUpdateStatus;

public record BulkUpdateUserStatusCommand(List<Guid> UserIds, UserStatus TargetStatus);



