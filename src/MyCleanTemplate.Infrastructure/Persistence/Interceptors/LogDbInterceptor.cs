using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Text.Json;
using System.Diagnostics;

namespace MyCleanTemplate.Infrastructure.Persistence.Interceptors;

internal class LogDbInterceptor : DbCommandInterceptor
{
    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result)
    {
        SaveLog(command);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        SaveLog(command);
        return new ValueTask<InterceptionResult<int>>(result);
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        SaveLog(command);
        return result;
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        SaveLog(command);
        return new ValueTask<InterceptionResult<DbDataReader>>(result);
    }

    public override InterceptionResult<object> ScalarExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result)
    {
        SaveLog(command);
        return result;
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        SaveLog(command);
        return new ValueTask<InterceptionResult<object>>(result);
    }

    private static void SaveLog(DbCommand command)
    {
        var parameters = command.Parameters.Cast<DbParameter>().ToList();

        Activity.Current?.SetTag("Query.TextQuery", command.CommandText);
        Activity.Current?.SetTag("Query.Arguments", JsonSerializer.Serialize(parameters.Select(a => new { a.ParameterName, a.Value })));
    }
}