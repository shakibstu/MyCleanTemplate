using Microsoft.AspNetCore.Authorization;

namespace MyCleanTemplate.Infrastructure.Authentication.Attributes;

/// <summary>
/// Authorization attribute that requires a specific permission based on service, feature, and action.
/// </summary>
public class MustHavePermissionAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Creates a new instance of <see cref="MustHavePermissionAttribute"/>.
    /// </summary>
    /// <param name="service">The service name (e.g., Identity).</param>
    /// <param name="feature">The feature name (e.g., Users, Roles).</param>
    /// <param name="action">The action name (e.g., Create, Read, Update, Delete).</param>
    public MustHavePermissionAttribute(string service, string feature, string action)
    {
        Policy = AppPermission.NameFor(service, feature, action);
    }
}
