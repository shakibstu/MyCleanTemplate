using MyCleanTemplate.Application.Contract.Abstractions.Messaging;

namespace MyCleanTemplate.Application.Contract.Test.Commands;

public sealed record CreateTestCommand(string Name, DateTime Date) : ICommand;
