using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Automation.SharedKernel.Abstractions.Querying;

namespace Automation.SharedKernel.Infrastructure.Querying;

public class PagedQueryBinder<T> : IRequestBinder<T> where T : PagedQuery, new()
{
    public ValueTask<T> BindAsync(BinderContext ctx, CancellationToken ct)
    {
        var req = new T();
        var q = ctx.HttpContext.Request.Query;

        if (int.TryParse(q["page"], out var p)) req.Page = p;
        if (int.TryParse(q["pageSize"], out var ps)) req.PageSize = ps;
        
        var kw = q["globalKeyword"];
        if (kw.Count > 0) req.GlobalKeyword = kw.ToString();

        var filters = new List<FilterField>();
        int i = 0;
        while (true)
        {
            var fieldKey = GetFirstValue(q, $"filters[{i}].field", $"filters[{i}].Field", $"filters[{i}][field]", $"filters[{i}][Field]");
            if (string.IsNullOrEmpty(fieldKey)) break;

            var opKey = GetFirstValue(q, $"filters[{i}].operator", $"filters[{i}].Operator", $"filters[{i}][operator]", $"filters[{i}][Operator]");
            var valKey = GetFirstValue(q, $"filters[{i}].value", $"filters[{i}].Value", $"filters[{i}][value]", $"filters[{i}][Value]");

            var opStr = opKey ?? "";
            FilterOperator op = FilterOperator.Equal;
            if (int.TryParse(opStr, out var intOp))
            {
                op = (FilterOperator)intOp;
            }
            else if (Enum.TryParse<FilterOperator>(opStr, true, out var enumOp))
            {
                op = enumOp;
            }
            
            filters.Add(new FilterField
            {
                Field = fieldKey!,
                Operator = op,
                Value = valKey
            });
            i++;
        }
        
        if (filters.Count > 0) req.Filters = filters;

        var sortDict = new Dictionary<string, bool>();
        foreach (var key in q.Keys)
        {
            var lowerKey = key.ToLowerInvariant();
            if (lowerKey.StartsWith("sort.") || lowerKey.StartsWith("sort["))
            {
                string field;
                if (lowerKey.StartsWith("sort.")) 
                    field = key.Substring(5);
                else 
                    field = key.Substring(5).TrimEnd(']'); // extracts "createdAt" from "sort[createdAt]"

                if (bool.TryParse(q[key].ToString(), out var isAsc))
                {
                    sortDict[field] = isAsc;
                }
            }
        }
        
        if (sortDict.Count > 0) req.Sort = sortDict;

        return ValueTask.FromResult(req);
    }

    private string? GetFirstValue(IQueryCollection q, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (q.ContainsKey(key))
            {
                return q[key].ToString();
            }
        }
        return null;
    }
}



