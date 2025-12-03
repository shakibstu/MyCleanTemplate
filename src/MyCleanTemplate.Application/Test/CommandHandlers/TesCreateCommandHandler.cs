
using MyCleanTemplate.Application.Contract.Test.Commands;
using MyCleanTemplate.Domain.Abstractions.Messaging;
using MyCleanTemplate.Domain.Test;

using Void = MediatRR.Contract.Messaging.Void;

namespace MyCleanTemplate.Application.Test.CommandHandlers;

public class TesCreateCommandHandler(ITestRepository testRepository) : ICommandHandler<CreateTestCommand>
{
    public async Task<Void> Handle(CreateTestCommand request, CancellationToken cancellationToken)
    {
        await testRepository.CreateTest(request.Name);
        return Void.Value;
        //throw new ForbiddenException("Test Exception", new Dictionary<string, string[]> { { "test", ["1","2"] } });
    }
}