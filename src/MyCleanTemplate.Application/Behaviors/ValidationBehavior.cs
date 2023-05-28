using FluentValidation;
using MediatR;
using MyCleanTemplate.Common.Dependencies;
using MyCleanTemplate.Domain.Exceptions.Base;

namespace MyCleanTemplate.Application.Behaviors;

internal class ValidationBehavior<TRequest, TResponse> : ITransientDependency, IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any()) return await next();
        var context = new ValidationContext<TRequest>(request);
        
        var validationResults = await Task.WhenAll(_validators
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
            throw new BadRequestException("Validation Failed!" ,errorsDictionary);
        }
        return await next();
    }
}
