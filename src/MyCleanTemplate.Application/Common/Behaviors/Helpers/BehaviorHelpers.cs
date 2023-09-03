using MediatR;
using MyCleanTemplate.Domain.Abstractions;

namespace MyCleanTemplate.Application.Common.Behaviors.Helpers;

internal static class BehaviorHelpers
{
    internal static async Task<TResponse> SaveChangesTransactional<TResponse>(this IUnitOfWork unitOfWork, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken = default)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        var response = await next();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return response;
    }
}