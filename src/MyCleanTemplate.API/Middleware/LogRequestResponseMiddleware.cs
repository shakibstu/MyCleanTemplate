using MyCleanTemplate.Domain.Common.Logs;
using Serilog.Context;
using ServiceGenerator;
using System.Text.Json;
using ILogger = Serilog.ILogger;
using LogLevel = MyCleanTemplate.Domain.Common.Logs.LogLevel;

namespace MyCleanTemplate.API.Middleware
{
    public class LogRequestResponseMiddleware(ILogger logger) : IMiddleware, ITransientJustImplementationDependency
    {
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
            using (LogContext.PushProperty("API.RequestBody", bodyAsText))
            using (LogContext.PushProperty("API.QueryParams", JsonSerializer.Serialize(context.Request.Query.Select(a => new { a.Key, a.Value }))))
            using (LogContext.PushProperty("API.ActionName", context.GetEndpoint()?.DisplayName))
            using (LogContext.PushProperty("API.Path", context.Request.Path))
            {
                logger.Information($"API.Request -> {context.GetEndpoint()?.DisplayName ?? ""}");
            }

            await next(context);

            stream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(stream).ReadToEndAsync();
            stream.Seek(0, SeekOrigin.Begin);

            await stream.CopyToAsync(originalResponseBody);

            context.Response.Body = originalResponseBody;

            using (LogContext.PushProperty(nameof(LogKeywords.SourceContext), LogKeywords.SourceContext))
            using (LogContext.PushProperty(nameof(LogLevel), LogLevel.ResponseApi))
            using (LogContext.PushProperty("API.ResponseBody", responseBody))
            using (LogContext.PushProperty("API.ActionName", context.GetEndpoint()?.DisplayName))
            using (LogContext.PushProperty("API.Path", context.Request.Path))
            {
                logger.Information($"API.Response -> {context.GetEndpoint()?.DisplayName ?? ""}");
            }

        }
    }
}
