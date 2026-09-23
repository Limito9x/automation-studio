using Automation.SharedKernel.Abstractions.Cursor;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Text;
using System.Globalization;

namespace Automation.SharedKernel.Infrastructure.Cursor;

public static class CursorPagingExtensions
{
    private static string EncodeCursor<TKey>(TKey key, Guid id) =>
    Convert.ToBase64String(Encoding.UTF8.GetBytes($"{FormatKey(key)}|{id}"));

    private static string FormatKey<TKey>(TKey key) => key switch
    {
        DateTimeOffset dto => dto.ToString("O"), // ISO 8601 round-trip chu?n
        _ => key!.ToString()!
    };

    private static (TKey Key, Guid Id) DecodeCursor<TKey>(string cursor)
    {
        var parts = Encoding.UTF8.GetString(Convert.FromBase64String(cursor)).Split('|');
        TKey key = typeof(TKey) == typeof(DateTimeOffset)
            ? (TKey)(object)DateTimeOffset.Parse(parts[0], null, DateTimeStyles.RoundtripKind)
            : (TKey)Convert.ChangeType(parts[0], typeof(TKey));
        return (key, Guid.Parse(parts[1]));
    }

    private sealed class ParameterReplacer(ParameterExpression from, ParameterExpression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) => node == from ? to : node;
    }


    public static async Task<CursorPage<TDto>> ToCursorPageAsync<TEntity, TKey, TDto>(
        this IQueryable<TEntity> query,
        Expression<Func<TEntity, TKey>> orderKey,      // VD: n => n.CreatedAt
        Expression<Func<TEntity, Guid>> tieBreaker,     // VD: n => n.Id
        Func<TEntity, TDto> mapper,
        CursorParam param,
        CancellationToken ct = default
    ) where TKey : IComparable<TKey>
    {
        query = query.OrderByDescending(orderKey).ThenByDescending(tieBreaker);

        var limit = param.Limit;
        var cursor = param.Cursor;
        
        if (cursor is { Length: > 0 })
        {
            var (key, id) = DecodeCursor<TKey>(cursor);
            var expressionParam = orderKey.Parameters[0];
            var keyBody = orderKey.Body;
            var replacer = new ParameterReplacer(tieBreaker.Parameters[0], expressionParam);
            var idBody = replacer.Visit(tieBreaker.Body)!;

            var lessThanKey = Expression.LessThan(keyBody, Expression.Constant(key)); // x => x.CreatedAt < cursor.Key
            var equalKey = Expression.Equal(keyBody, Expression.Constant(key)); // x => x.CreatedAt == cursor.Key
            var lessThanId = Expression.LessThan(idBody, Expression.Constant(id)); // x => x.Id < cursor.Id
            var condition = Expression.OrElse(lessThanKey, Expression.AndAlso(equalKey, lessThanId)); // x => x.CreatedAt < cursor.Key || (x.CreatedAt == cursor.Key && x.Id < cursor.Id)

            query = query.Where(Expression.Lambda<Func<TEntity, bool>>(condition, expressionParam));
        }

        var items = await query.Take(limit + 1).ToListAsync(ct);
        var hasMore = items.Count > limit;
        var page = items.Take(limit).ToList();

        string? nextCursor = null;
        if (hasMore)
        {
            var last = page[^1];
            var lastKey = orderKey.Compile()(last);
            var lastId = tieBreaker.Compile()(last);
            nextCursor = EncodeCursor(lastKey, lastId);
        }

        return new CursorPage<TDto>(page.Select(mapper).ToList(), nextCursor, hasMore);
    }
}


