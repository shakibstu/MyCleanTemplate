using Microsoft.EntityFrameworkCore.Diagnostics;
using MyCleanTemplate.Domain.Common.Logs;
using Serilog;
using Serilog.Context;
using System.Data.Common;
using System.Text.Json;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace MyCleanTemplate.Infrastructure.Persistence.Interceptors
{
    internal class LogDbInterceptor(ILogger logger, TracerProvider tracerProvider) : DbCommandInterceptor
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

        private void SaveLog(DbCommand command)
        {
            var parameters = command.Parameters.Cast<DbParameter>().ToList();

            Activity.Current?.SetTag("Query.TextQuery", command.CommandText);
            Activity.Current?.SetTag("Query.Arguments", JsonSerializer.Serialize(parameters.Select(a => new { a.ParameterName, a.Value })));

            using (LogContext.PushProperty(nameof(LogKeywords.SourceContext), LogKeywords.SourceContext))
            using (LogContext.PushProperty(nameof(LogLevel), LogLevel.DbQuery))
            using (LogContext.PushProperty("Query.TextQuery", command.CommandText))
            using (LogContext.PushProperty("Query.Arguments", JsonSerializer.Serialize(parameters.Select(a => new { a.ParameterName, a.Value }))))
            {
                logger.Information(@"Query Run");
            }
        }
    }
}
