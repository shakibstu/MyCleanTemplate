using MyCleanTemplate.Domain.Abstractions.Messaging;

namespace MyCleanTemplate.Application.Contract.Auth.Commands;

public sealed record EnableMfaCommand : ICommand
{
    public string UserId { get; init; } = string.Empty;
    public string MfaCode { get; init; } = string.Empty;
}
