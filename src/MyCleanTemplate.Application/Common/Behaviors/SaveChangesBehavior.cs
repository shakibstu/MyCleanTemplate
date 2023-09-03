using MediatR;
using MyCleanTemplate.Application.Common.Behaviors.Helpers;
using MyCleanTemplate.Application.Contract.Abstractions.Messaging;
using MyCleanTemplate.Common.Dependencies;
using MyCleanTemplate.Domain.Abstractions;

namespace MyCleanTemplate.Application.Common.Behaviors
{
    internal class SaveChangesBehaviorWithResponse<TRequest, TResponse> : ITransientDependency, IPipelineBehavior<TRequest, TResponse> where TRequest : ICommand<TResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public SaveChangesBehaviorWithResponse(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            return _unitOfWork.SaveChangesTransactional(next, cancellationToken);
        }
    }
    internal class SaveChangesBehaviorWithoutResponse<TRequest, TResponse> : ITransientDependency, IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, ICommand
    {
        private readonly IUnitOfWork _unitOfWork;

        public SaveChangesBehaviorWithoutResponse(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            return _unitOfWork.SaveChangesTransactional(next, cancellationToken);
        }
    }
}
