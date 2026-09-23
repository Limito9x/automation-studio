using Microsoft.AspNetCore.Identity;
using Automation.SharedKernel.Domain.Interfaces;

namespace Automation.Identity.Domain;

public class Role : IdentityRole<Guid>, IAuditable, IAuditTrackable
{
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}



