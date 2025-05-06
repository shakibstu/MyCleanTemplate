using MediatR;
using MyCleanTemplate.Domain.Common.Logs;
using System.Text.Json;
using ServiceGenerator;

namespace MyCleanTemplate.Application.Common.Behaviors;

[TransientDependency(typeof(IPipelineBehavior<,>))]
internal class TracingBehavior<TRequest, TResponse> : 
    IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestType = LogHelper.GetFullName(request.GetType());

        using var activity =
            TelemetryHelper.MainActivitySource.StartActivity(TelemetryHelper.TelemetrySpanLevelApp);

        activity?.SetTag("APP.RequestType", requestType);
        activity?.SetTag("APP.Request", request);

        var response = await next(cancellationToken);
        
        var responseType = LogHelper.GetFullName(response?.GetType());

        activity?.SetTag("APP.ResponseType", responseType);
        activity?.SetTag("APP.Response", JsonSerializer.Serialize(response));

        return response;
    }
}