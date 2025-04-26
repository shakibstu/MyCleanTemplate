using MediatR;
using MyCleanTemplate.Application.Contract.Abstractions.Messaging;
using MyCleanTemplate.Application.Contract.Test.Commands;
using MyCleanTemplate.Domain.Test;

namespace MyCleanTemplate.Application.Test.CommandHandlers;

public class TesCreateCommandHandler(ITestRepository testRepository) : ICommandHandler<CreateTestCommand>
{
    public async Task Handle(CreateTestCommand request, CancellationToken cancellationToken)
    {
        await testRepository.CreateTest(request.Name);
    }
}