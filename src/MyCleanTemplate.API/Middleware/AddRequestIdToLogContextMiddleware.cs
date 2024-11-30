using MyCleanTemplate.Domain.Common.Logs;
using Serilog.Context;
using ServiceGenerator;

namespace MyCleanTemplate.API.Middleware;

[TransientDependency<AddRequestIdToLogContextMiddleware>]
internal class AddRequestIdToLogContextMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        using (LogContext.PushProperty(LogKeywords.ApiRId, Guid.NewGuid()))
        {
            await next.Invoke(context);
        }
    }
}