namespace Automation.Pipeline.Engine.Models;

public class ExecSegment
{
    public Guid SegmentId { get; init; } = Guid.NewGuid();
    public string Executor { get; init; } = "dotNet";
    public List<ExecStep> Steps { get; init; } = [];
    public bool IsFlowControl { get; init; }
    public ExecPlan? BodyPlan { get; set; }
    public ExecPlan? ContinuationPlan { get; set; }

    public ExecSegment() { }

    public ExecSegment(string executor, bool isFlowControl = false)
    {
        Executor = executor;
        IsFlowControl = isFlowControl;
    }
}
