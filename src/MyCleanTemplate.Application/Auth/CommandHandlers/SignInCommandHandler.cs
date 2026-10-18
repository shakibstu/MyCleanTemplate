using MyCleanTemplate.Application.Contract.Auth;
using MyCleanTemplate.Application.Contract.Auth.Commands;
using MyCleanTemplate.Domain.Abstractions.Messaging;
using MyCleanTemplate.Domain.Exceptions.Base;

namespace MyCleanTemplate.Application.Auth.CommandHandlers;

public class SignInCommandHandler(IIdentityService identityService) : ICommandHandler<SignInCommand, AuthToken>
{
    public async Task<AuthToken> Handle(SignInCommand request, CancellationToken cancellationToken)
    {
        var user = await identityService.FindByNameAsync(request.Username);
        if (user is null || !await identityService.CheckPasswordAsync(user.Id, request.Password))
        {
            throw new UnAuthorizedException("Invalid username or password.", new Dictionary<string, string[]>());
        }

        // A user with MFA enabled never gets a token from the password alone.
        if (user.TwoFactorEnabled)
        {
            if (string.IsNullOrEmpty(request.MfaCode))
            {
                throw new UnAuthorizedException("MFA code is required.",
                    new Dictionary<string, string[]> { { "requiresMfa", ["true"] } });
            }

            if (!await identityService.VerifyMfaCodeAsync(user.Id, request.MfaCode))
            {
                throw new UnAuthorizedException("Invalid MFA code.", new Dictionary<string, string[]>());
            }
        }

        await identityService.ResetFailedAttemptsAsync(user.Id);
        return await identityService.IssueTokenAsync(user.Id, mfaVerified: user.TwoFactorEnabled);
    }
}
