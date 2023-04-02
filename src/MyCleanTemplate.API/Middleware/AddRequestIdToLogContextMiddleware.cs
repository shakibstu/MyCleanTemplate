using Serilog.Context;
using MyCleanTemplate.Common.Logs;

namespace MyCleanTemplate.API.Middleware
{
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
}
