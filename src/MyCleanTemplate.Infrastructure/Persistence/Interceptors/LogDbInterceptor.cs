using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MyCleanTemplate.Common.Logs;
using Newtonsoft.Json;
using Serilog;
using Serilog.Context;

namespace MyCleanTemplate.Infrastructure.Persistence.Interceptors
{
    internal class LogDbInterceptor : DbCommandInterceptor
    {
        private readonly ILogger _logger;
        public LogDbInterceptor(ILogger logger)
        {
            _logger = logger;
        }
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

            using (LogContext.PushProperty(nameof(LogLevel), LogLevel.DB))
            {
                _logger.Information(command.CommandText + Environment.NewLine + JsonConvert.SerializeObject(new
                {
                    Parameters = parameters.Select(a => new { a.ParameterName, a.Value }),
                }));
            }
        }
    }
}
