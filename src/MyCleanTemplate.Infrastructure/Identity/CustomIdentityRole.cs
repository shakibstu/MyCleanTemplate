using Microsoft.AspNetCore.Identity;

namespace MyCleanTemplate.Infrastructure.Identity;

public class CustomIdentityRole : IdentityRole
{
    public List<string>? Permissions { get; set; }
}