using System.Text.Json.Serialization;

namespace Automation.SharedKernel.Abstractions.Querying;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FilterOperator
{
    Equal,
    NotEqual,
    Contains,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
}



