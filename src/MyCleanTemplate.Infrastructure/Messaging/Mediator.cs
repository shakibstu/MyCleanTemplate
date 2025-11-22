using Microsoft.Extensions.DependencyInjection;
using MyCleanTemplate.Domain.Abstractions.Messaging;
using MyCleanTemplate.Domain.Exceptions.Base;
using ServiceGenerator;
using Void = MyCleanTemplate.Domain.Abstractions.Messaging.Void;

namespace MyCleanTemplate.Infrastructure.Messaging;

[TransientDependency<IMediator>]
internal class Mediator(IServiceProvider serviceProvider) : IMediator
{
    private async Task<TResponse> SendWithResponse<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));

        if (handlerType == null)
        {
            throw new BadRequestException($"No Handler Defined for {request.GetType()}",
                new Dictionary<string, string[]>());
        }
        var handler = serviceProvider.GetRequiredService(handlerType);

        const string handleName = nameof(IRequestHandler<IRequest>.Handle);
        return await ((Task<TResponse>)handler.GetType().GetMethod(handleName)!.Invoke(
            handler, [request, cancellationToken]))!;
    }

    private async Task SendWithoutResponse<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
    {
        var handler = serviceProvider.GetService<IRequestHandler<TRequest>>();

        if (handler == null)
        {
            throw new BadRequestException($"No Handler Defined for {request.GetType()}",
                new Dictionary<string, string[]>());
        }

        await handler.Handle(request, cancellationToken);
    }

    private void PublishToChannel<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        serviceProvider.GetRequiredService<NotificationChannel>().AddToChannel(notification, cancellationToken);
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        return await Execute(request, () => SendWithResponse(request, cancellationToken), MediatorActionType.RequestWithResponse, cancellationToken);
    }

    private async Task<TResponse> Execute<TRequest, TResponse>(TRequest request, Func<Task<TResponse>> handler, MediatorActionType actionType, CancellationToken cancellationToken = default)
    {
        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        var behaviors = serviceProvider.GetServices(behaviorType);

        var response = behaviors.Reverse()
            .Aggregate(handler,
                (next, behavior) => () =>
                    (Task<TResponse>)behavior.GetType()
                        .GetMethod(nameof(IPipelineBehavior<IRequest<TResponse>, TResponse>.Handle))!
                        .Invoke(behavior, [request, next, actionType, cancellationToken])).Invoke();
        return await response;
    }
    private async Task ExecuteNotification<TRequest>(TRequest request, Func<Task> handler, CancellationToken cancellationToken = default)
    {
        var behaviorType = typeof(INotificationBehavior<>).MakeGenericType(request.GetType());
        var behaviors = serviceProvider.GetServices(behaviorType);

        var response = behaviors.Reverse()
            .Aggregate(handler,
                (next, behavior) => () =>
                    (Task)behavior.GetType()
                        .GetMethod(nameof(INotificationBehavior<INotification>.Handle))!
                        .Invoke(behavior, [request, next, cancellationToken])).Invoke();
        await response;
    }

    public async Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
    {
        await Execute(request, InternalSend, request is ICommand ? MediatorActionType.Command : MediatorActionType.Request, cancellationToken);
        return;

        async Task<Void> InternalSend()
        {
            await SendWithoutResponse(request, cancellationToken);
            return new Void();
        }
    }

    public async Task Publish<TNotification>(TNotification notification,
        CancellationToken cancellationToken = default) where TNotification : INotification
    {
        await ExecuteNotification(notification, InternalPublish, cancellationToken);
        return;

        Task InternalPublish()
        {
            PublishToChannel(notification, cancellationToken);
            return Task.FromResult(new Void());
        }

    }
}