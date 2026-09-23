using Automation.Pipeline.Engine.StructRegistry;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Pipelines.GetStructCatalogue;

[NonTransactional]
public class GetStructCatalogueHandler(IEntityStructRegistry structRegistry)
{
    public Task<Result<IReadOnlyList<StructDefinitionDto>>> HandleAsync(
        GetStructCatalogueQuery query,
        CancellationToken ct
    )
    {
        var allStructs = structRegistry.GetAll(query.ProjectId);
        var dtos = allStructs
            .Select(s => new StructDefinitionDto(s.StructType, s.Label, s.OutputPins))
            .ToList();

        return Task.FromResult(Result.Ok<IReadOnlyList<StructDefinitionDto>>(dtos));
    }
}
