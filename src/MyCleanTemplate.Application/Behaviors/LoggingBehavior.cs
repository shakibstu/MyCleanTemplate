using MediatR;
using MyCleanTemplate.Common.Dependencies;
using MyCleanTemplate.Common.Logs;
using Serilog;
using Serilog.Context;
using System.Text.Json;

namespace MyCleanTemplate.Application.Behaviors
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
            using (LogContext.PushProperty("App.ActionName", typeof(TRequest).FullName))
            {

                _logger.Information(JsonSerializer.Serialize(request));

                response = await next();

                using (LogContext.PushProperty(nameof(LogKeywords.SourceContext), LogKeywords.SourceContext))
                using (LogContext.PushProperty(nameof(LogLevel), LogLevel.ResponseApp))
                using (LogContext.PushProperty("App.ActionName", typeof(TRequest).FullName))
                {
                    _logger.Information(JsonSerializer.Serialize(response));
                }
            }
            return response;
        }
    }
}
