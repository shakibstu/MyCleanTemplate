using Microsoft.AspNetCore.Authorization;

namespace MyCleanTemplate.Infrastructure.Authentication.Permissions
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                return Task.CompletedTask;
            }

            // No issuer filter: claims carry the token's "iss" after validation, and the
            // JWT signature already guarantees they were issued by us.
            var permitted = context.User.HasClaim(AppClaim.Permission, requirement.RequiredPermission);

            if (permitted)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
