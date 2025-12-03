
using FluentValidation;
using MediatRR.Contract.Messaging;
using MyCleanTemplate.Domain.Abstractions.Messaging;
using ServiceGenerator;
using ValidationException = MyCleanTemplate.Domain.Exceptions.ValidationException;

namespace MyCleanTemplate.Application.Common.Behaviors;

[TransientDependency(typeof(IPipelineBehavior<,>))]
internal class ValidationBehavior<TRequest, TResponse>
    (IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken)
    {
        if (!validators.Any()) return await next();
        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(validators
            .Select(x => x.ValidateAsync(context, cancellationToken)));

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
        return await next();
    }
}
