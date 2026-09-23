using Automation.Pipeline.Features.Nodes.CreateCustomNode;
using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Nodes.GetCustomNodeById;

[NonTransactional]
public class GetCustomNodeByIdHandler(PipelineDbContext db)
{
    public async Task<Result<CreateCustomNodeResponseDto>> HandleAsync(
        GetCustomNodeByIdQuery query,
        CancellationToken ct
    )
    {
        var node = await db.NodeDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == query.Id, ct);

        if (node == null)
        {
            return Result.Fail<CreateCustomNodeResponseDto>("Custom node not found.");
        }

        return Result.Ok(new CreateCustomNodeResponseDto(
            node.Id,
            node.ProjectId,
            node.Name,
            node.Key,
            node.Label,
            node.Executor,
            node.Inputs,
            node.Outputs,
            node.CreatedAt
        ));
    }
}
