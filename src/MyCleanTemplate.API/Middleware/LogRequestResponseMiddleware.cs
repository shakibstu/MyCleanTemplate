using MyCleanTemplate.Common.Logs;
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


            context.Request.EnableBuffering();
            var bodyAsText = await new StreamReader(context.Request.Body).ReadToEndAsync();
            context.Request.Body.Position = 0;

            using (LogContext.PushProperty(nameof(LogKeywords.SourceContext), LogKeywords.SourceContext))
            using (LogContext.PushProperty(nameof(LogLevel), LogLevel.RequestInApi))
            using (LogContext.PushProperty("Body", bodyAsText))
            using (LogContext.PushProperty("API.ActionName", context.GetEndpoint()?.DisplayName))
            {
                _logger.Information(context.Request.Path);
            }

            await next(context);

            stream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(stream).ReadToEndAsync();
            stream.Seek(0, SeekOrigin.Begin);

            await stream.CopyToAsync(originalResponseBody);

            context.Response.Body = originalResponseBody;

            using (LogContext.PushProperty(nameof(LogKeywords.SourceContext), LogKeywords.SourceContext))
            using (LogContext.PushProperty(nameof(LogLevel), LogLevel.ResponseApi))
            using (LogContext.PushProperty("Body", responseBody))
            using (LogContext.PushProperty("API.ActionName", context.GetEndpoint()?.DisplayName))
            {
                _logger.Information(context.Request.Path);
            }

        }
    }
}
