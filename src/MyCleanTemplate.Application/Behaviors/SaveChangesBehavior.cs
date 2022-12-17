using MediatR;
using MyCleanTemplate.Application.Behaviors.Helpers;
using MyCleanTemplate.Application.Contract.Abstractions.Messaging;
using MyCleanTemplate.Domain;
using MyCleanTemplate.Domain.Abstractions;

namespace MyCleanTemplate.Application.Behaviors
{
    internal class SaveChangesBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : ICommand, IRequest<TResponse> where TResponse : class , ITransientDependency
    {
        private readonly IUnitOfWork _unitOfWork;

        public SaveChangesBehavior(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            return _unitOfWork.SaveChangesTransactional(next, cancellationToken);
        }
    }
    internal class SaveChangesBehaviorForNotifiers<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : INotificationCommand, IRequest<TResponse> where TResponse : class , ITransientDependency
    {
        private readonly IUnitOfWork _unitOfWork;

        public SaveChangesBehaviorForNotifiers(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            return _unitOfWork.SaveChangesTransactional(next, cancellationToken);
        }
    }
}
