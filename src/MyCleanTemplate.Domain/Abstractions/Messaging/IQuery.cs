namespace MyCleanTemplate.Domain.Abstractions.Messaging;

public interface IQuery<out TResponse> : IRequest<TResponse>;