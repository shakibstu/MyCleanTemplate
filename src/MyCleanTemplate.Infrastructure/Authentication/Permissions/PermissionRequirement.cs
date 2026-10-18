using Microsoft.AspNetCore.Authorization;

namespace MyCleanTemplate.Infrastructure.Authentication.Permissions
{
    public class PermissionRequirement(string requiredPermission) : IAuthorizationRequirement
    {
        public string RequiredPermission { get; } = requiredPermission;
    }
}
