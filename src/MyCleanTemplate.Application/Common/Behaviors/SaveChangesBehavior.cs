using MediatR;
using MyCleanTemplate.Application.Common.Behaviors.Helpers;
using MyCleanTemplate.Application.Contract.Abstractions.Messaging;
using MyCleanTemplate.Domain.Abstractions;
using ServiceGenerator;

namespace MyCleanTemplate.Application.Common.Behaviors;

[TransientDependency(typeof(IPipelineBehavior<,>))]
internal class SaveChangesBehaviorWithResponse<TRequest, TResponse>(IUnitOfWork unitOfWork) : 
    IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return unitOfWork.SaveChangesTransactional(next, cancellationToken);
    }
}
[TransientDependency(typeof(IPipelineBehavior<,>))]
internal class SaveChangesBehaviorWithoutResponse<TRequest, TResponse>
    (IUnitOfWork unitOfWork) :
    IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ICommand
{
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return unitOfWork.SaveChangesTransactional(next, cancellationToken);
    }
}