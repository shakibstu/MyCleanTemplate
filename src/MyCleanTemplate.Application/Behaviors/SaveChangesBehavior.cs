using MediatR;
using MyCleanTemplate.Domain;
using MyCleanTemplate.Domain.Abstractions;

namespace MyCleanTemplate.Application.Behaviors
{
    internal class SaveChangesBehavior<TRequest, TResponse, T> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse> where TResponse : Result<T> 
    {
        private readonly IUnitOfWork _unitOfWork;

        public SaveChangesBehavior(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var response = await next();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return response;
        }

        public Task<Result<TResponse>> Handle(TRequest request, RequestHandlerDelegate<Result<TResponse>> next, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
