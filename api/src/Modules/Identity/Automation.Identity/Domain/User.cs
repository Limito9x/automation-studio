using Automation.Identity.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Automation.SharedKernel.Domain.Interfaces;

namespace Automation.Identity.Domain;

public class User : IdentityUser<Guid>, IAuditable, ISoftDelete, IAuditTrackable
{
    public User()
    {
        Id = Guid.CreateVersion7();
        SecurityStamp = Guid.NewGuid().ToString();
    }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public UserStatus Status { get; set; } = UserStatus.Active;
    public bool MustChangePassword { get; set; } = false;

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];

    // Auditable
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // SoftDelete
    public bool IsDeleted => DeletedAt.HasValue;
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    [AuditIgnore]
    public override string? PasswordHash { get; set; }

    [AuditIgnore]
    public override string? SecurityStamp { get; set; }

    [AuditIgnore]
    public override string? ConcurrencyStamp { get; set; }
}



