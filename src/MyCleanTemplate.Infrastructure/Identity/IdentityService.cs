using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MyCleanTemplate.Application.Auth;
using MyCleanTemplate.Application.Contract.Auth;
using MyCleanTemplate.Infrastructure.Authentication;
using ServiceGenerator;

namespace MyCleanTemplate.Infrastructure.Identity;

[ScopedDependency<IIdentityService>]
internal class IdentityService(
    UserManager<CustomIdentityUser> userManager,
    RoleManager<CustomIdentityRole> roleManager,
    ITokenService tokenService,
    IOptions<JwtSettings> jwtSettings) : IIdentityService
{
    public async Task<IdentityUserInfo?> FindByNameAsync(string userName) =>
        ToInfo(await userManager.FindByNameAsync(userName));

    public async Task<IdentityUserInfo?> FindByIdAsync(string userId) =>
        ToInfo(await userManager.FindByIdAsync(userId));

    public async Task<bool> CheckPasswordAsync(string userId, string password)
    {
        var user = await GetUserAsync(userId);
        if (await userManager.IsLockedOutAsync(user)) return false;

        if (await userManager.CheckPasswordAsync(user, password)) return true;

        await userManager.AccessFailedAsync(user);
        return false;
    }

    public async Task<bool> VerifyMfaCodeAsync(string userId, string code)
    {
        var user = await GetUserAsync(userId);
        if (await userManager.IsLockedOutAsync(user)) return false;

        if (await userManager.VerifyTwoFactorTokenAsync(user, userManager.Options.Tokens.AuthenticatorTokenProvider, code))
            return true;

        await userManager.AccessFailedAsync(user);
        return false;
    }

    public async Task ResetFailedAttemptsAsync(string userId) =>
        await userManager.ResetAccessFailedCountAsync(await GetUserAsync(userId));

    public async Task<string> GetOrCreateMfaKeyAsync(string userId)
    {
        var user = await GetUserAsync(userId);
        var key = await userManager.GetAuthenticatorKeyAsync(user);
        if (!string.IsNullOrEmpty(key)) return key;

        await userManager.ResetAuthenticatorKeyAsync(user);
        return (await userManager.GetAuthenticatorKeyAsync(user))!;
    }

    public async Task EnableMfaAsync(string userId) =>
        await userManager.SetTwoFactorEnabledAsync(await GetUserAsync(userId), true);

    public async Task<AuthToken> IssueTokenAsync(string userId, bool mfaVerified)
    {
        var user = await GetUserAsync(userId);

        var roleNames = await userManager.GetRolesAsync(user);
        var roles = (await Task.WhenAll(roleNames.Select(roleManager.FindByNameAsync)))
            .OfType<CustomIdentityRole>()
            .ToList();

        var token = tokenService.GenerateToken(
            user.Id,
            user.UserName!,
            user.Email!,
            mfaVerified,
            roles.Select(r => r.Name ?? r.Id),
            roles.SelectMany(r => r.Permissions ?? []).Union(user.Permissions ?? []));

        return new AuthToken(token, jwtSettings.Value.TokenExpirationMinutes * 60);
    }

    private async Task<CustomIdentityUser> GetUserAsync(string userId) =>
        await userManager.FindByIdAsync(userId) ?? throw new InvalidOperationException($"User '{userId}' not found.");

    private static IdentityUserInfo? ToInfo(CustomIdentityUser? user) =>
        user is null ? null : new IdentityUserInfo(user.Id, user.UserName!, user.TwoFactorEnabled);
}
