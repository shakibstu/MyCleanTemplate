using MyCleanTemplate.Domain.Abstractions.Messaging;
namespace MyCleanTemplate.Application.Contract.Test.Commands;

public sealed record CreateTestCommand(string Name) : ICommand;
