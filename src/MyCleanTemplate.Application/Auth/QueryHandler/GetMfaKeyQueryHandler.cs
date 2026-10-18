using MyCleanTemplate.Application.Contract.Auth.Query;
using MyCleanTemplate.Domain.Abstractions.Messaging;
using MyCleanTemplate.Domain.Exceptions.Base;

namespace MyCleanTemplate.Application.Auth.QueryHandler;

public class GetMfaKeyQueryHandler(IIdentityService identityService) : IQueryHandler<GetMfaKeyQuery, string>
{
    public async Task<string> Handle(GetMfaKeyQuery request, CancellationToken cancellationToken)
    {
        var user = await identityService.FindByIdAsync(request.UserId)
                   ?? throw new UnAuthorizedException("User not found.", new Dictionary<string, string[]>());

        // Once MFA is enabled the shared secret must never be handed out again,
        // otherwise a password-only token could be used to mint valid codes.
        if (user.TwoFactorEnabled)
        {
            throw new ForbiddenException("MFA is already enabled.", new Dictionary<string, string[]>());
        }

        return await identityService.GetOrCreateMfaKeyAsync(user.Id);
    }
}
