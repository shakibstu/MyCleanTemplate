using MyCleanTemplate.Domain.Abstractions.Messaging;

namespace MyCleanTemplate.Application.Contract.Auth.Commands;

public sealed record SignInCommand : ICommand<AuthToken>
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Authenticator code. Required when the user has MFA enabled.
    /// </summary>
    public string? MfaCode { get; init; }
}
