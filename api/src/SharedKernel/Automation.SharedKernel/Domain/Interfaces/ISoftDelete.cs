namespace Automation.SharedKernel.Domain.Interfaces;

public interface ISoftDelete
{
    bool IsDeleted { get; }
    DateTimeOffset? DeletedAt { get; set; }
    string? DeletedBy { get; set; }
}


