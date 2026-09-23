namespace Automation.SharedKernel.Abstractions.Querying;

public abstract class PagedQuery
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public Dictionary<string, bool>? Sort { get; set; }  // field -> isAscending
    public List<FilterField>? Filters { get; set; }
    public string? GlobalKeyword { get; set; }
}



