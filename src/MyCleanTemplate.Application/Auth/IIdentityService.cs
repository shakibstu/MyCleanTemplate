using MyCleanTemplate.Application.Contract.Auth;

namespace MyCleanTemplate.Application.Auth;

public sealed record IdentityUserInfo(string Id, string UserName, bool TwoFactorEnabled);

/// <summary>
/// Port to the identity store. Implemented in Infrastructure.
/// </summary>
public interface IIdentityService
{
    Task<IdentityUserInfo?> FindByNameAsync(string userName);
    Task<IdentityUserInfo?> FindByIdAsync(string userId);

    /// <summary>
    /// Checks the password, counting failures towards lockout. Returns false when locked out.
    /// </summary>
    Task<bool> CheckPasswordAsync(string userId, string password);

    /// <summary>
    /// Verifies an authenticator (TOTP) code, counting failures towards lockout. Returns false when locked out.
    /// </summary>
    Task<bool> VerifyMfaCodeAsync(string userId, string code);

    Task ResetFailedAttemptsAsync(string userId);
    Task<string> GetOrCreateMfaKeyAsync(string userId);
    Task EnableMfaAsync(string userId);
    Task<AuthToken> IssueTokenAsync(string userId, bool mfaVerified);
}
