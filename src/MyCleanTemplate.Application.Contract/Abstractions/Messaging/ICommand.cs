using MediatR;
using MyCleanTemplate.Domain;

namespace MyCleanTemplate.Application.Contract.Abstractions.Messaging
{
    public interface ICommand : IRequest<Result>
    {
    }

    public interface ICommand<TResponse> : IRequest<Result<TResponse>>
    {
    }
}
