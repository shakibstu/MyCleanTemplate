using MyCleanTemplate.Domain.Abstractions.Messaging;
namespace MyCleanTemplate.Application.Contract.Test.Commands;

public sealed record CreateTestCommand : ICommand
{
    public string Name { get; init; }
}
