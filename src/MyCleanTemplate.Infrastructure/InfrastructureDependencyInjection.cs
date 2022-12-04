using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyCleanTemplate.Domain.Common.Interfaces;
using MyCleanTemplate.Infrastructure.Persistence;

namespace MyCleanTemplate.Infrastructure
{
    public static class InfrastructureDependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

            services.Scan(scan => scan
                .FromCallingAssembly()
                .AddClasses(classes => classes.AssignableTo(typeof(IBaseRepository<>)))
                .AsMatchingInterface()
                .WithScopedLifetime()
            );

            return services;
        }
    }
}
