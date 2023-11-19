using MediatR;
using MyCleanTemplate.Domain.Common.Logs;
using Serilog.Context;
using ServiceGenerator;
using System.Text.Json;
using ILogger = Serilog.ILogger;

namespace MyCleanTemplate.Application.Common.Behaviors
{
    internal class LoggingBehavior<TRequest, TResponse>(ILogger logger) : ITransientDependency,
        IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            TResponse? response;
            using (LogContext.PushProperty(nameof(LogKeywords.SourceContext), LogKeywords.SourceContext))
            using (LogContext.PushProperty(LogKeywords.AppRId, Guid.NewGuid()))
            using (LogContext.PushProperty(nameof(LogLevel), LogLevel.RequestInApp))
            using (LogContext.PushProperty("APP.ActionName", request.GetType().Name))
            using (LogContext.PushProperty("APP.Request", JsonSerializer.Serialize(request)))
            {
                logger.Information($"APP.RequestType -> {request.GetType().Name}");
                response = await next();
            }
            using (LogContext.PushProperty(nameof(LogKeywords.SourceContext), LogKeywords.SourceContext))
            using (LogContext.PushProperty(nameof(LogLevel), LogLevel.ResponseApp))
            using (LogContext.PushProperty("APP.Response", JsonSerializer.Serialize(response)))
            {
                logger.Information($"APP.ResponseType -> {response?.GetType().Name}");
            }

            return response;
        }
    }
}
