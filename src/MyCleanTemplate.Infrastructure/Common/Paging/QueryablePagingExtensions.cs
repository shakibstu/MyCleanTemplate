using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using MyCleanTemplate.Application.Contract.Common.Paging;

namespace MyCleanTemplate.Infrastructure.Common.Paging;

public static class QueryablePagingExtensions
{
    public static async Task<Page<T>> ToPageAsync<T>(
        this IQueryable<T> source,
        PageRequest request,
        CancellationToken cancellationToken = default)
    {
        var ordered = string.IsNullOrWhiteSpace(request.SortBy)
            ? source
            : source.OrderByProperty(request.SortBy!, request.SortDirection);

        var total = await source.LongCountAsync(cancellationToken);
        if (total == 0)
        {
            return Page<T>.Empty(request);
        }

        var items = await ordered
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new Page<T>(items, request.Page, request.PageSize, total);
    }

    private static IOrderedQueryable<T> OrderByProperty<T>(
        this IQueryable<T> source,
        string propertyName,
        SortDirection direction)
    {
        var property = typeof(T).GetProperty(
            propertyName,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
            ?? throw new ArgumentException(
                $"Property '{propertyName}' not found on {typeof(T).Name}.", nameof(propertyName));

        var parameter = Expression.Parameter(typeof(T), "x");
        var access = Expression.MakeMemberAccess(parameter, property);
        var lambda = Expression.Lambda(access, parameter);

        var methodName = direction == SortDirection.Desc ? "OrderByDescending" : "OrderBy";
        var call = Expression.Call(
            typeof(Queryable),
            methodName,
            [typeof(T), property.PropertyType],
            source.Expression,
            Expression.Quote(lambda));

        return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(call);
    }
}
