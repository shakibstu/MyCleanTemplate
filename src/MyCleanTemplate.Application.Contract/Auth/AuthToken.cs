namespace MyCleanTemplate.Application.Contract.Auth;

/// <param name="Token">The JWT.</param>
/// <param name="ExpiresIn">Token lifetime in seconds.</param>
/// <param name="TokenType">Always "Bearer".</param>
public sealed record AuthToken(string Token, int ExpiresIn, string TokenType = "Bearer");
