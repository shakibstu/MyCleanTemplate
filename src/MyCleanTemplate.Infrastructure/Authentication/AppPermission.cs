using System.Collections.ObjectModel;

namespace MyCleanTemplate.Infrastructure.Authentication
{
    public record AppPermission(string Service, string Feature, string Action, string Group, string Description, bool IsBasic = false)
    {
        public const string Prefix = "Permission.";

        public string Name => NameFor(Service, Feature, Action);

        public static string NameFor(string service, string feature, string action) =>
            $"{Prefix}{service}.{feature}.{action}";
    }
    public static class AppPermissions
    {
        private static readonly AppPermission[] AllPermissionsInternal =
        [
            new(AppService.Identity, AppFeature.Users, AppAction.Create, AppRoleGroup.SystemAccess, "Create Users"),
            new (AppService.Identity, AppFeature.Users, AppAction.Read, AppRoleGroup.SystemAccess, "Read Users"),
            new (AppService.Identity, AppFeature.Users, AppAction.Delete, AppRoleGroup.SystemAccess, "Delete Users"),
            new (AppService.Identity, AppFeature.Users, AppAction.Update, AppRoleGroup.SystemAccess, "Update Users"),

            new(AppService.Identity, AppFeature.Roles, AppAction.Create, AppRoleGroup.SystemAccess, "Create Roles"),
            new (AppService.Identity, AppFeature.Roles, AppAction.Read, AppRoleGroup.SystemAccess, "Read Roles"),
            new (AppService.Identity, AppFeature.Roles, AppAction.Delete, AppRoleGroup.SystemAccess, "Delete Roles"),
            new (AppService.Identity, AppFeature.Roles, AppAction.Update, AppRoleGroup.SystemAccess, "Update Roles"),

            new(AppService.Identity, AppFeature.UserRoles, AppAction.Update, AppRoleGroup.SystemAccess, "Update User Roles"),
            new (AppService.Identity, AppFeature.UserRoles, AppAction.Read, AppRoleGroup.SystemAccess, "Read User Roles"),

            new (AppService.Identity, AppFeature.RoleClaims, AppAction.Update, AppRoleGroup.SystemAccess, "Update Role Claims/Permission"),
            new (AppService.Identity, AppFeature.RoleClaims, AppAction.Read, AppRoleGroup.SystemAccess, "Read Role Claims/Permission"),
        ];

        public static IReadOnlyList<AppPermission> AllPermissions =>
            new ReadOnlyCollection<AppPermission>(AllPermissionsInternal);

        public static IReadOnlyList<AppPermission> AdminPermissions =>
            new ReadOnlyCollection<AppPermission>(AllPermissionsInternal.Where(a => !a.IsBasic).ToList());

        public static IReadOnlyList<AppPermission> BasicPermissions =>
            new ReadOnlyCollection<AppPermission>(AllPermissionsInternal.Where(a => a.IsBasic).ToList());

    }
}
