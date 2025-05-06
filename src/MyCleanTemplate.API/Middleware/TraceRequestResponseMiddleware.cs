using System.Diagnostics;
using MyCleanTemplate.Domain.Common.Logs;
using ServiceGenerator;
using System.Text.Json;

namespace MyCleanTemplate.API.Middleware;

[TransientDependency<LogRequestResponseMiddleware>]
public class LogRequestResponseMiddleware: IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        using var activity =
            TelemetryHelper.MainActivitySource.StartActivity(TelemetryHelper.TelemetrySpanLevelApi);
        try
        {

            using var stream = new MemoryStream();
            var originalResponseBody = context.Response.Body;
            context.Response.Body = stream;

            context.Request.EnableBuffering();
            var bodyAsText = await new StreamReader(context.Request.Body).ReadToEndAsync();
            context.Request.Body.Position = 0;

            activity?.SetTag("API.RequestBody", bodyAsText);
            activity?.SetTag("API.QueryParams",
                JsonSerializer.Serialize(context.Request.Query.Select(a => new { a.Key, a.Value })));
            activity?.SetTag("API.ActionName", context.GetEndpoint()?.DisplayName);
            activity?.SetTag("API.Path", context.Request.Path);

            await next(context);

            stream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(stream).ReadToEndAsync();
            stream.Seek(0, SeekOrigin.Begin);

            await stream.CopyToAsync(originalResponseBody);

            context.Response.Body = originalResponseBody;

            activity?.SetTag("API.ResponseBody", responseBody);
            activity?.SetTag("API.ActionName", context.GetEndpoint()?.DisplayName);
            activity?.SetTag("API.Path", context.Request.Path);
        }
        catch (Exception e)
        {
            activity?.AddException(e);
            activity?.SetStatus(ActivityStatusCode.Error, e.Message);
            throw;
        }
    }
}