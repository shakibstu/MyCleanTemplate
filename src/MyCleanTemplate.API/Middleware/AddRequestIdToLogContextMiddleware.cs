using MyCleanTemplate.Common.Dependencies;
using MyCleanTemplate.Common.Logs;
using Serilog.Context;

namespace MyCleanTemplate.API.Middleware
{
    internal class AddRequestIdToLogContextMiddleware : IMiddleware, ITransientJustImplementationDependency
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
