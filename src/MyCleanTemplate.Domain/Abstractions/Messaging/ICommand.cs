
using MediatRR.Contract.Messaging;
using Void = MediatRR.Contract.Messaging.Void;

namespace MyCleanTemplate.Domain.Abstractions.Messaging;

public interface ICommand : IRequest<Void>;
public interface ICommand<out TResponse> : IRequest<TResponse>;