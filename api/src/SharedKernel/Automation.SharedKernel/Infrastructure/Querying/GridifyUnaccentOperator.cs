using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Globalization;
using Gridify;
using Gridify.Syntax;
using Microsoft.EntityFrameworkCore;

namespace Automation.SharedKernel.Infrastructure.Querying;

public class GridifyUnaccentOperator : IGridifyOperator
{
    public string GetOperator() => "#==*";

    public Expression<OperatorParameter> OperatorHandler()
    {
        var efFunctions = Expression.Constant(EF.Functions);

        var unaccentMethod = typeof(NpgsqlFullTextSearchDbFunctionsExtensions).GetMethod(
            nameof(NpgsqlFullTextSearchDbFunctionsExtensions.Unaccent),
            new[] { typeof(DbFunctions), typeof(string) }
        );

        var iLikeMethod = typeof(NpgsqlDbFunctionsExtensions).GetMethod(
            nameof(NpgsqlDbFunctionsExtensions.ILike),
            new[] { typeof(DbFunctions), typeof(string), typeof(string) }
        );

        if (unaccentMethod == null || iLikeMethod == null)
        {
            return (prop, val) => (prop as string)!.Contains((val as string)!);
        }

        var propParam = Expression.Parameter(typeof(object), "prop");
        var valParam = Expression.Parameter(typeof(object), "val");

        var propString = Expression.Convert(propParam, typeof(string));
        var valString = Expression.Convert(valParam, typeof(string));

        var unaccentProp = Expression.Call(null, unaccentMethod, efFunctions, propString);
        var unaccentVal = Expression.Call(null, unaccentMethod, efFunctions, valString);

        var concatMethod = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string), typeof(string) })!;
        var pattern = Expression.Call(concatMethod, Expression.Constant("%"), unaccentVal, Expression.Constant("%"));

        var iLikeCall = Expression.Call(null, iLikeMethod, efFunctions, unaccentProp, pattern);

        return Expression.Lambda<OperatorParameter>(iLikeCall, propParam, valParam);
    }
}

