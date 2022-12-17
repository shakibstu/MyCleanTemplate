using MediatR;
using MyCleanTemplate.Application.Contract.Abstractions.Messaging;
using MyCleanTemplate.Application.Contract.Test.Commands;

namespace MyCleanTemplate.Application.Test.CommandHandlers;

public class TesCreateCommandHandler : ICommandHandler<CreateTestCommand>
{
    public Task<Unit> Handle(CreateTestCommand request, CancellationToken cancellationToken)
    {
        Console.WriteLine("s");
        return Unit.Task;
    }
}