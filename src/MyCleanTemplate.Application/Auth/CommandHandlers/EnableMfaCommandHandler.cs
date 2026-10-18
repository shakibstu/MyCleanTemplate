using MyCleanTemplate.Application.Contract.Auth.Commands;
using MyCleanTemplate.Domain.Abstractions.Messaging;
using MyCleanTemplate.Domain.Exceptions.Base;

using Void = MediatRR.Contract.Messaging.Void;

namespace MyCleanTemplate.Application.Auth.CommandHandlers;

public class EnableMfaCommandHandler(IIdentityService identityService) : ICommandHandler<EnableMfaCommand>
{
    public async Task<Void> Handle(EnableMfaCommand request, CancellationToken cancellationToken)
    {
        var user = await identityService.FindByIdAsync(request.UserId)
                   ?? throw new UnAuthorizedException("User not found.", new Dictionary<string, string[]>());

        if (user.TwoFactorEnabled)
        {
            throw new BadRequestException("MFA is already enabled.", new Dictionary<string, string[]>());
        }

        if (!await identityService.VerifyMfaCodeAsync(user.Id, request.MfaCode))
        {
            throw new ForbiddenException("Invalid MFA code.", new Dictionary<string, string[]>());
        }

        await identityService.EnableMfaAsync(user.Id);
        return Void.Value;
    }
}
