using System.Text.Json;

namespace Automation.DynamicForms.Features.Structs.CreateStruct;

public record CreateStructCommand(Guid ProjectId, string Name, JsonDocument? Fields = null);
