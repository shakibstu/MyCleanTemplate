using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyCleanTemplate.Domain.Abstractions;
using MyCleanTemplate.Infrastructure.Authentication;
using MyCleanTemplate.Infrastructure.Authentication.Permissions;
using MyCleanTemplate.Infrastructure.Identity;
using MyCleanTemplate.Infrastructure.Persistence;
using ServiceGenerator;
namespace MyCleanTemplate.Infrastructure;

public static class InfrastructureServiceRegistration
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructureServices(IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>((_, options) =>
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

            return services.InjectFromSourceGenerator().AddAuth(configuration);

        }

        private IServiceCollection AddAuth(IConfiguration configuration)
        {
            // Fail at startup, not on the first request, when the Jwt section is missing or invalid
            services.AddOptions<JwtSettings>()
                .Bind(configuration.GetSection(JwtSettings.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();
            services.AddScoped<ITokenService, TokenService>();

            // Identity without its cookie schemes; this API authenticates with JWT only
            services.AddIdentityCore<CustomIdentityUser>()
                .AddRoles<CustomIdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
            services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<JwtSettings>>((options, jwt) =>
                {
                    var settings = jwt.Value;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = settings.Issuer,
                        ValidAudience = settings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey)),
                        ClockSkew = TimeSpan.FromSeconds(settings.ClockSkewSeconds)
                    };
                });

            // Named policies plus permission-based ones. AddAuthorization registers the default policy
            // provider with TryAdd, so the PermissionPolicyProvider below is the one that gets resolved;
            // it falls back to the default provider for named policies such as AdminOnly.
            services.AddAuthorizationBuilder()
                .AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
                    policy.RequireAuthenticatedUser().RequireRole("Admin"));
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
            return services;
        }
    }

    public static async Task ConfigureInfrastructure(this IServiceProvider serviceProvider)
    {
        var scope = serviceProvider.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if ((await db.Database.GetPendingMigrationsAsync()).Any())
        {
            await db.Database.MigrateAsync();
        }

        var seeders = scope.ServiceProvider.GetServices<IDataSeeder>();
        var dataSeeders = seeders as IReadOnlyList<IDataSeeder> ?? seeders.ToList();
        dataSeeders = dataSeeders.OrderBy(o => o.Order).ToList();

        foreach (var seeder in dataSeeders)
        {
            await seeder.SeedAsync();
        }
        await db.SaveChangesAsync();
    }
}