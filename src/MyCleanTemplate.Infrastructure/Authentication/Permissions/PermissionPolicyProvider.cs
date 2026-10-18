using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace MyCleanTemplate.Infrastructure.Authentication.Permissions
{
    public class PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : IAuthorizationPolicyProvider
    {
        private DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; } = new(options);

        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            if (!policyName.StartsWith(AppPermission.Prefix, StringComparison.Ordinal))
            {
                return FallbackPolicyProvider.GetPolicyAsync(policyName);
            }

            var policy = new AuthorizationPolicyBuilder();
            
            policy.AddRequirements(new PermissionRequirement(policyName));
            
            return Task.FromResult(policy.Build());
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
            FallbackPolicyProvider.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy> GetFallbackPolicyAsync() =>
            FallbackPolicyProvider.GetFallbackPolicyAsync();
    }
}
