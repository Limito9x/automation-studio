using Automation.Agent.Contracts;
using Wolverine.Attributes;

namespace Automation.Agent.Features.Agents.DiscoverAgentFolder;

[NonTransactional]
public class DiscoverAgentFolderHandler(IAgentApi agentApi)
{
    public async Task<Result<DiscoverAgentFolderResult>> HandleAsync(DiscoverAgentFolderQuery request, CancellationToken ct)
    {
        var targetPath = request.Path?.Trim() ?? string.Empty;
        var result = await agentApi.SendBrowseCommandAsync(request.Id, targetPath, ct);
        if (result.IsFailed)
        {
            return result.ToResult();
        }

        var items = (result.Value.Items ?? [])
            .Where(x => x.IsDirectory)
            .Select(x => new DirectoryNodeDto(x.Name, x.Path))
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Result.Ok(new DiscoverAgentFolderResult(
            result.Value.CurrentPath,
            result.Value.ParentPath,
            result.Value.CanNavigateUp,
            items
        ));
    }
}