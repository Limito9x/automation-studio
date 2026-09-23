namespace Automation.Pipeline.Domain.Enums;

public enum ExecutionStatus
{
    Pending = 1,
    Running = 2,
    WaitingForAgent = 3,
    Succeeded = 4,
    Failed = 5,
    Cancelled = 6
}


