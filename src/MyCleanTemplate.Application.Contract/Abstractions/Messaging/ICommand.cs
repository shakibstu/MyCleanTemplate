using MediatR;
using MyCleanTemplate.Domain;

namespace MyCleanTemplate.Application.Contract.Abstractions.Messaging
{
    public interface ICommand : IRequest
    {
    }
    public interface ICommand<out TResponse> : IRequest<TResponse>
    {
    }
}
