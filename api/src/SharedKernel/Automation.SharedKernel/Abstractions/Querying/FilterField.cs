namespace Automation.SharedKernel.Abstractions.Querying;

public class FilterField
{
    public required string Field { get; set; }
    public required FilterOperator Operator { get; set; }
    public required string Value { get; set; }
}



