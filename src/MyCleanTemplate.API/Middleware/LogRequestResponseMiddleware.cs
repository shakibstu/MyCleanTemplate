using Serilog.Context;
using ILogger = Serilog.ILogger;
using LogLevel = MyCleanTemplate.Common.Logs.LogLevel;

namespace MyCleanTemplate.API.Middleware
{
    public class LogRequestResponseMiddleware : IMiddleware
    {
        private readonly ILogger _logger;
        public LogRequestResponseMiddleware(ILogger logger)
        {
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            using var stream = new MemoryStream();
            var originalResponseBody = context.Response.Body;
            context.Response.Body = stream;

            using (LogContext.PushProperty(nameof(LogLevel), nameof(LogLevel.Rinqapi)))
            {
                context.Request.EnableBuffering();
                var bodyAsText = await new StreamReader(context.Request.Body).ReadToEndAsync();
                context.Request.Body.Position = 0;

                using (LogContext.PushProperty("Body", bodyAsText))
                {
                    _logger.Information(context.Request.Path);
                }
            }

            await next(context);

            using (LogContext.PushProperty(nameof(LogLevel), nameof(LogLevel.Rinpapi)))
            {
                stream.Seek(0, SeekOrigin.Begin);
                var responseBody = await new StreamReader(stream).ReadToEndAsync();
                stream.Seek(0, SeekOrigin.Begin);
                
                await stream.CopyToAsync(originalResponseBody);

                context.Response.Body = originalResponseBody;

                using (LogContext.PushProperty("Body", responseBody))
                {
                    _logger.Information(context.Request.Path);
                }

            }
        }
    }
}
