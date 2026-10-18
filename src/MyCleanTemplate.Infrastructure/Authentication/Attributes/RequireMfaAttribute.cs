using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using MyCleanTemplate.Infrastructure.Identity;

namespace MyCleanTemplate.Infrastructure.Authentication.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireMfaAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Not authenticated" });
            return;
        }

        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<CustomIdentityUser>>();

        var appUser = await userManager.GetUserAsync(user);
        if (appUser == null)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "User not found" });
            return;
        }
        
        var mfaEnabled = await userManager.GetTwoFactorEnabledAsync(appUser);
        
        // Check if user has MFA enabled in the database
        if (!mfaEnabled)
        {
            context.Result = new ObjectResult(new
            {
                message = "MFA is required to access this resource. Please enable multi-factor authentication on your account.",
                requiresMfaSetup = true
            })
            {
                StatusCode = 403
            };
            return;
        }

        // Check if this token was issued after MFA verification
        var mfaVerifiedClaim = user.FindFirst(AppClaim.MfaVerified)?.Value;
        var mfaVerified = bool.TryParse(mfaVerifiedClaim, out var verified) && verified;

        if (!mfaVerified)
        {
            context.Result = new ObjectResult(new
            {
                message = "MFA verification required. Please login again with your authenticator code.",
                requiresMfa = true
            })
            {
                StatusCode = 403
            };
        }
    }
}