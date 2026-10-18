using Microsoft.AspNetCore.Identity;

namespace MyCleanTemplate.Infrastructure.Identity;

public class CustomIdentityUser : IdentityUser
{
    public List<string>? Permissions { get; set; }
}

