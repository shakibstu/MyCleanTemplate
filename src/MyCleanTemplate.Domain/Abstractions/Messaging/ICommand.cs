
namespace MyCleanTemplate.Domain.Abstractions.Messaging;

public interface ICommand : IRequest;
public interface ICommand<out TResponse> : IRequest<TResponse>;