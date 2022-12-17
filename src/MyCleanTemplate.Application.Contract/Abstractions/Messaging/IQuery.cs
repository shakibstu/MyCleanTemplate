using MediatR;

namespace MyCleanTemplate.Application.Contract.Abstractions.Messaging;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}