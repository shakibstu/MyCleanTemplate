using MediatRR.Contract.Messaging;

namespace MyCleanTemplate.Domain.Abstractions.Messaging;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>;