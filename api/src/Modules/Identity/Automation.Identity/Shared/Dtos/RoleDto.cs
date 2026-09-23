namespace Automation.Identity.Shared.Dtos;

public record RoleDto(
    Guid Id, 
    string Name,
    DateTimeOffset CreatedAt,
    string? CreatedBy,
    DateTimeOffset? UpdatedAt,
    string? UpdatedBy
);



