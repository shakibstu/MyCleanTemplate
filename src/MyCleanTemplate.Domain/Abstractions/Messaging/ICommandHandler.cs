using MediatRR.Contract.Messaging;
using Void = MediatRR.Contract.Messaging.Void;

namespace MyCleanTemplate.Domain.Abstractions.Messaging;

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Void> where TCommand : ICommand;

public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>;