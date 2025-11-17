using Azure.Core;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MyCleanTemplate.Domain.Abstractions.Messaging;
using MyCleanTemplate.Domain.Common.Logs;
using MyCleanTemplate.Domain.Exceptions.Base;
using ServiceGenerator;
using System.Diagnostics;
using System.Text.Json;
using ValidationException = MyCleanTemplate.Domain.Exceptions.ValidationException;
using Void = MyCleanTemplate.Domain.Abstractions.Messaging.Void;

namespace MyCleanTemplate.Infrastructure.Messaging;

[TransientDependency<IMediator>]
internal class Mediator(IServiceProvider serviceProvider) : IMediator
{
    private async Task<TResponse> SendWithResponse<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        Type handlerType = null;
        await Validate(request.GetType(), request, cancellationToken);
        if (request.GetType().IsAssignableTo(typeof(IQuery<TResponse>)))
        {
            handlerType = typeof(IQueryHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        }
        else if (request.GetType().IsAssignableTo(typeof(ICommand<TResponse>)))
        {
            handlerType = typeof(ICommandHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        }

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
        await Validate(request.GetType(), request, cancellationToken);

        var handlerType = request is ICommand
            ? typeof(ICommandHandler<>).MakeGenericType(request.GetType())
            : typeof(IRequestHandler<TRequest>);

        var handler = serviceProvider.GetService(handlerType);

        if (handler == null)
        {
            throw new BadRequestException($"No Handler Defined for {request.GetType()}",
                new Dictionary<string, string[]>());
        }

        await ((IRequestHandler<TRequest>)handler).Handle(request, cancellationToken);
    }

    private void PublishToChannel<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        serviceProvider.GetRequiredService<NotificationChannel>().AddToChannel(notification, cancellationToken);
    }

    private async Task Validate(Type requestType, object request, CancellationToken cancellationToken)
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(requestType);
        var validators = serviceProvider.GetServices(validatorType)
            .Cast<IValidator>()
            .ToArray();
        if (!validators.Any()) return;
        var validationContextType = typeof(ValidationContext<>).MakeGenericType(requestType);
        var validationContext = (IValidationContext)Activator.CreateInstance(validationContextType, request);

        var validationResults = await Task.WhenAll(validators.Select(a => a.ValidateAsync(validationContext, cancellationToken)));

        var errorsDictionary = validationResults
            .SelectMany(x => x.Errors)
            .Where(x => x != null)
            .GroupBy(
                x => x.PropertyName,
                x => x.ErrorMessage,
                (propertyName, errorMessages) => new
                {
                    Key = propertyName,
                    Values = errorMessages.Distinct().ToArray()
                })
            .ToDictionary(x => x.Key, x => x.Values);
        if (errorsDictionary.Any())
        {
            throw new ValidationException("Validation Failed!", errorsDictionary);
        }
    }

    private static async Task<TResponse> HandleAndTelemetry<TResponse>(Func<Task<TResponse>> handler, IBaseRequest request, MediatorActionType actionType,Activity activity)
    {
        try
        {
            activity?.SetTag("Mediator.Action.Type", Enum.GetName(actionType));
            activity?.SetTag("Mediator.Request.Type", request.GetType().ToString());
            activity?.SetTag("Mediator.Request.Value", JsonSerializer.Serialize(request, request.GetType()));

            var response = await handler.Invoke();

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

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {

        return await Execute(request, () => SendWithResponse(request, cancellationToken), MediatorActionType.RequestWithResponse, cancellationToken);

        //using var activity = TelemetryHelper.MainActivitySource.StartActivity(TelemetryHelper.TelemetrySpanLevelMediator);
        //var response = await HandleAndTelemetry(() => SendWithResponse(request, cancellationToken), request, MediatorActionType.RequestWithResponse, activity);
    }

    private async Task<TResponse> Execute<TRequest,TResponse>(TRequest request,Func<Task<TResponse>> handler, MediatorActionType actionType, CancellationToken cancellationToken = default)
    {
        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        var behaviors = serviceProvider.GetServices(behaviorType);

        var response = behaviors.Reverse()
            .Aggregate(handler,
                (next, behavior) => () =>
                    (Task<TResponse>)behavior.GetType()
                        .GetMethod(nameof(IPipelineBehavior<IRequest<TResponse>, TResponse>.Handle))!
                        .Invoke(behavior, [request, next,actionType, cancellationToken])).Invoke();
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