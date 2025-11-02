using MyCleanTemplate.Application.Contract.Test.Commands;
using MyCleanTemplate.Domain.Abstractions.Messaging;
using MyCleanTemplate.Domain.Exceptions.Base;
using MyCleanTemplate.Domain.Test;

namespace MyCleanTemplate.Application.Test.CommandHandlers;

public class TesCreateCommandHandler(ITestRepository testRepository) : ICommandHandler<CreateTestCommand>
{
    public async Task Handle(CreateTestCommand request, CancellationToken cancellationToken)
    {
        await testRepository.CreateTest(request.Name);
        //throw new ForbiddenException("Test Exception", new Dictionary<string, string[]> { { "test", ["1","2"] } });
    }
}