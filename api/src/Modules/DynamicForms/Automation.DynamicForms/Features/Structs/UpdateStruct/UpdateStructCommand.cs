using System.Text.Json;

namespace Automation.DynamicForms.Features.Structs.UpdateStruct;

public record UpdateStructCommand(Guid ProjectId, Guid Id, string Name, JsonDocument? Fields = null);
