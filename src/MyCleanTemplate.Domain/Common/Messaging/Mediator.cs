
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MyCleanTemplate.Domain.Abstractions.Messaging;
using MyCleanTemplate.Domain.Exceptions.Base;
using ServiceGenerator;
using ValidationException = MyCleanTemplate.Domain.Exceptions.ValidationException;
namespace MyCleanTemplate.Domain.Common.Messaging
{
    [TransientDependency<IMediator>]
    internal class Mediator(IServiceProvider serviceProvider) : IMediator
    {
        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
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
            return await ((Task<TResponse>)handler.GetType().GetMethod("Handle")!.Invoke(handler, [request, cancellationToken]))!;
        }

        public async Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
        {
            Type handlerType = null;

            await Validate(request.GetType(), request, cancellationToken);
            if (request.GetType().IsAssignableTo(typeof(ICommand)))
            {
                handlerType = typeof(ICommandHandler<>).MakeGenericType(request.GetType());
            }
            else if (request.GetType().IsAssignableTo(typeof(IRequest)))
            {
                handlerType = typeof(IRequestHandler<>).MakeGenericType(request.GetType());
            }
            if (handlerType == null)
            {
                throw new BadRequestException($"No Handler Defined for {request.GetType()}",
                    new Dictionary<string, string[]>());
            }
            var handler = serviceProvider.GetRequiredService(handlerType);
            await ((Task)handler.GetType().GetMethod("Handle")!.Invoke(handler, [request, cancellationToken]))!;
        }

        private async Task Validate(Type requestType, object request, CancellationToken cancellationToken)
        {
            var validatorType = typeof(IValidator<>).MakeGenericType(requestType);
            var validators = serviceProvider.GetServices(validatorType)
                .Cast<IValidator>()
                .ToArray();
            if (!validators.Any()) return;
            var validationContextType = typeof(ValidationContext<>).MakeGenericType(requestType);
            var validationContext = (IValidationContext) Activator.CreateInstance(validationContextType, request);

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
    }
}
