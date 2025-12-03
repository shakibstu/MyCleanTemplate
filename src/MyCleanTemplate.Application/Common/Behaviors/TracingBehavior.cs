using MyCleanTemplate.Domain.Abstractions.Messaging;
using MyCleanTemplate.Domain.Common.Logs;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using ServiceGenerator;
using System.Diagnostics;
using System.Text.Json;
using MediatRR.Contract.Messaging;
using MyCleanTemplate.Domain.Common;

namespace MyCleanTemplate.Application.Common.Behaviors;

[TransientDependency(typeof(IPipelineBehavior<,>))]
internal class RequestTracingBehavior<TRequest, TResponse> :
    IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken)
    {
        using var activity = TelemetryHelper.MainActivitySource.StartActivity(TelemetryHelper.TelemetrySpanLevelMediator);
        try
        {
            activity?.SetTag("Mediator.Action.Type", ActionType(request));
            activity?.SetTag("Mediator.Request.Type", request.GetType().ToString());
            activity?.SetTag("Mediator.Request.Value", JsonSerializer.Serialize(request, request.GetType()));

            var response = await next();

            activity?.SetTag("Mediator.Response.Type", response.GetType().ToString());
            activity?.SetTag("Mediator.Response.Value", JsonSerializer.Serialize(response, response.GetType()));

            return response;
        }
        catch (Exception e)
        {
            activity?.AddException(e);
            activity?.SetStatus(ActivityStatusCode.Error, e.Message);
            throw;
        }
    }
    private static MediatorActionType ActionType<TRequest>(TRequest request) => request switch
    {
        _ when request is ICommand<TResponse> => MediatorActionType.CommandWithResponse,
        _ when request is IQuery<TResponse> => MediatorActionType.RequestWithResponse,
        _ when request is ICommand => MediatorActionType.Command,
        _ => MediatorActionType.Request
    };
}

[TransientDependency(typeof(INotificationBehavior<>))]
internal class NotificationTracingBehavior<TNotification> :
    INotificationBehavior<TNotification>
    where TNotification : notnull
{
    public Task Handle(TNotification request, Func<Task> next, CancellationToken cancellationToken = default)
    {
        using var activity =
            TelemetryHelper.MainActivitySource.StartActivity(TelemetryHelper.TelemetrySpanLevelMediator,
                ActivityKind.Producer);
        if (request is BaseEvent evt)
        {
            evt.Properties ??= new Dictionary<string, object>();
            var propagationContext = new PropagationContext(
                activity?.Context ?? default,
                Baggage.Current);
            Propagators.DefaultTextMapPropagator.Inject(propagationContext, evt.Properties, (dictionary, key, value) =>
            {
                dictionary[key] = value;
            });
        }
        try
        {
            activity?.SetTag("Mediator.Action.Type", Enum.GetName(MediatorActionType.Notification));
            activity?.SetTag("Mediator.Request.Type", request.GetType().ToString());
            activity?.SetTag("Mediator.Request.Value", JsonSerializer.Serialize(request, request.GetType()));
            return next();
        }
        catch (Exception e)
        {
            activity?.AddException(e);
            activity?.SetStatus(ActivityStatusCode.Error, e.Message);
            throw;
        }
    }
}


[TransientDependency(typeof(INotificationHandlerBehavior<>))]
internal class NotificationHandlerTracingBehavior<TNotification> :
    INotificationHandlerBehavior<TNotification>
    where TNotification : notnull
{
    public async Task Handle(TNotification request, Func<Task> next, CancellationToken cancellationToken = default)
    {
        ActivityContext context = default;
        if (request is BaseEvent { Properties: not null } evt)
        {
            var parentContext = Propagators.DefaultTextMapPropagator.Extract(default, evt.Properties,
                (properties, key) =>
                {
                    if (properties.TryGetValue(key, out var value))
                    {
                        return [value?.ToString() ?? ""];
                    }

                    return [];
                });
            Baggage.Current = parentContext.Baggage;
            context = parentContext.ActivityContext;
        }

        using var activity = TelemetryHelper.MainActivitySource.StartActivity(
            TelemetryHelper.TelemetrySpanLevelMediator, ActivityKind.Consumer, context
        );
        try
        {
            activity?.SetTag("Mediator.Action.Type", "Consumer");
            activity?.SetTag("Mediator.Notification.Type", request.GetType().ToString());
            activity?.SetTag("Mediator.Notification.Value", JsonSerializer.Serialize(request, request.GetType()));
            await next();
        }
        catch (Exception e)
        {
            activity?.AddException(e);
            activity?.SetStatus(ActivityStatusCode.Error, e.Message);
        }
    }
}