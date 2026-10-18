using Microsoft.AspNetCore.Identity;
using MyCleanTemplate.Domain.Abstractions;
using MyCleanTemplate.Infrastructure.Authentication;
using ServiceGenerator;

namespace MyCleanTemplate.Infrastructure.Identity;

[TransientDependency<IDataSeeder>]
public class UserRoleSeeder(UserManager<CustomIdentityUser> userManager, RoleManager<CustomIdentityRole> roleManager) : IDataSeeder
{
    public int Order { get; init; }
    public async Task SeedAsync()
    {
        var adminRole = await roleManager.FindByIdAsync("Admin");
        if (adminRole == null)
        {
            adminRole = new CustomIdentityRole
            {
                Permissions = AppPermissions.AdminPermissions.Select(p => p.Name).ToList(),
                Name = "Admin",
                NormalizedName = "ADMIN",
                Id = "Admin"
            };
            await roleManager.CreateAsync(adminRole);
        }
        else
        {
            // Pick up permissions added to AppPermissions since the role was first seeded
            var missing = AppPermissions.AdminPermissions.Select(p => p.Name)
                .Except(adminRole.Permissions ?? [])
                .ToList();
            if (missing.Count > 0)
            {
                adminRole.Permissions = [.. adminRole.Permissions ?? [], .. missing];
                await roleManager.UpdateAsync(adminRole);
            }
        }

        if (await userManager.FindByIdAsync("Admin") == null)
        {
            var adminUser = new CustomIdentityUser
            {
                UserName = "Admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@example.com",
                NormalizedEmail = "ADMIN@EXAMPLE.COM",
                Id = "Admin",
            };
            await userManager.CreateAsync(adminUser, "Admin123!");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}