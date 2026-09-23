using Microsoft.AspNetCore.SignalR;

namespace Automation.Pipeline.Hubs;

public class PipelineExecutionHub : Hub
{
    public async Task JoinPipeline(string pipelineId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"pipeline_{pipelineId}");
    }

    public async Task LeavePipeline(string pipelineId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"pipeline_{pipelineId}");
    }

    public async Task JoinExecution(string executionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"execution_{executionId}");
    }

    public async Task LeaveExecution(string executionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"execution_{executionId}");
    }
}
