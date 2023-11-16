using MediatR;
using MyCleanTemplate.Common.Dependencies;
using MyCleanTemplate.Common.Logs;
using Serilog;
using Serilog.Context;
using System.Text.Json;

namespace MyCleanTemplate.Application.Common.Behaviors
{
    internal class LoggingBehavior<TRequest, TResponse> : ITransientDependency, IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly ILogger _logger;

        public LoggingBehavior(ILogger logger)
        {
            _logger = logger;
        }
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            TResponse? response;
            using (LogContext.PushProperty(nameof(LogKeywords.SourceContext), LogKeywords.SourceContext))
            using (LogContext.PushProperty(LogKeywords.AppRId, Guid.NewGuid()))
            using (LogContext.PushProperty(nameof(LogLevel), LogLevel.RequestInApp))
            using (LogContext.PushProperty("APP.ActionName", request.GetType().Name))
            using (LogContext.PushProperty("APP.Request", JsonSerializer.Serialize(request)))
            {

                _logger.Information($"APP.RequestType -> {request.GetType().Name}");

                response = await next();
            }
            using (LogContext.PushProperty(nameof(LogKeywords.SourceContext), LogKeywords.SourceContext))
            using (LogContext.PushProperty(nameof(LogLevel), LogLevel.ResponseApp))
            using (LogContext.PushProperty("APP.Response", JsonSerializer.Serialize(response)))
            {
                _logger.Information($"APP.ResponseType -> {response?.GetType().Name}");
            }

            return response;
        }
    }
}
