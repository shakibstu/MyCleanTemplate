using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MyCleanTemplate.Domain.Abstractions.Messaging;
using MyCleanTemplate.Domain.Common.Logs;
using MyCleanTemplate.Domain.Exceptions.Base;
using ServiceGenerator;
using System.Diagnostics;
using System.Text.Json;
using ValidationException = MyCleanTemplate.Domain.Exceptions.ValidationException;
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

    private void PublishToChannel<TNotification>(TNotification notification, Activity activity, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        serviceProvider.GetRequiredService<NotificationChannel>().AddToChannel(notification,activity, cancellationToken);
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
        using var activity = TelemetryHelper.MainActivitySource.StartActivity(TelemetryHelper.TelemetrySpanLevelMediator);
        var response = await HandleAndTelemetry(() => SendWithResponse(request, cancellationToken), request, MediatorActionType.RequestWithResponse, activity);
        return response;
    }

    public async Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
    {
        using var activity = TelemetryHelper.MainActivitySource.StartActivity(TelemetryHelper.TelemetrySpanLevelMediator);
        await HandleAndTelemetry(InternalSend, request,
            request is ICommand ? MediatorActionType.Command : MediatorActionType.Request, activity);
        return;

        async Task<string> InternalSend()
        {
            await SendWithoutResponse(request, cancellationToken);
            return "No Response";
        }
    }

    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        using var activity =
            TelemetryHelper.MainActivitySource.StartActivity(TelemetryHelper.TelemetrySpanLevelMediator,
                ActivityKind.Producer);
        await HandleAndTelemetry(InternalPublish, notification, MediatorActionType.Notification, activity);
        return;

        Task<string> InternalPublish()
        {
            PublishToChannel(notification, activity, cancellationToken);
            return Task.FromResult("No Response");
        }

    }
}