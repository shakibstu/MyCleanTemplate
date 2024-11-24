using MediatR;
using MyCleanTemplate.Domain.Common.Logs;
using Serilog.Context;
using System.Text.Json;
using ServiceGenerator;
using ILogger = Serilog.ILogger;

namespace MyCleanTemplate.Application.Common.Behaviors
{
    [TransientDependency(typeof(IPipelineBehavior<,>))]
    internal class LoggingBehavior<TRequest, TResponse>(ILogger logger) : 
        IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestType = LogHelper.GetFullName(request.GetType());

            using var activity =
                TelemetryProperties.MainActivitySource.StartActivity(LogKeywords.TelemetrySpanLevelApp);

            activity?.SetTag("APP.RequestType", requestType);
            activity?.SetTag("APP.Request", JsonSerializer.Serialize(request));

            TResponse? response;
            using (LogContext.PushProperty(nameof(LogKeywords.SourceContext), LogKeywords.SourceContext))
            using (LogContext.PushProperty(LogKeywords.AppRId, Guid.NewGuid()))
            using (LogContext.PushProperty(nameof(LogLevel), LogLevel.RequestInApp))
            using (LogContext.PushProperty("APP.ActionName", requestType))
            using (LogContext.PushProperty("APP.Request", JsonSerializer.Serialize(request)))
            {
                logger.Information($"APP.RequestType -> {requestType}");
                response = await next();
            }

            var responseType = LogHelper.GetFullName(response?.GetType());

            using (LogContext.PushProperty(nameof(LogKeywords.SourceContext), LogKeywords.SourceContext))
            using (LogContext.PushProperty(nameof(LogLevel), LogLevel.ResponseApp))
            using (LogContext.PushProperty("APP.Response", JsonSerializer.Serialize(response)))
            {
                logger.Information($"APP.ResponseType -> {responseType}");
            }

            activity?.SetTag("APP.ResponseType", responseType);
            activity?.SetTag("APP.Response", JsonSerializer.Serialize(response));

            return response;
        }
    }
}
