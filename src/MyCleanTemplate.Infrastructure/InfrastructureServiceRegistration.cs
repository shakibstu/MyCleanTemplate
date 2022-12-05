using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyCleanTemplate.Domain.Abstractions;
using MyCleanTemplate.Infrastructure.Persistence;

namespace MyCleanTemplate.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

            services.Scan(scan => scan
                .FromCallingAssembly()

                .AddClasses(classes => classes.AssignableTo(typeof(ITransientDependency)), false)
                .AsImplementedInterfaces()
                .WithTransientLifetime()

                .AddClasses(classes => classes.AssignableTo(typeof(ISingletonDependency)), false)
                .AsImplementedInterfaces()
                .WithSingletonLifetime()

                .AddClasses(classes => classes.AssignableTo(typeof(IScopeDependency)), false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            );

            return services;
        }

        public static async Task ConfigureInfrastructure(this IServiceProvider serviceProvider)
        {
            var scope = serviceProvider.CreateAsyncScope();

            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if ((await db.Database.GetPendingMigrationsAsync()).Any())
            {
                await db.Database.MigrateAsync();
            }

            var x =scope.ServiceProvider.GetRequiredService(typeof(IPipelineBehavior<,>));
            var seeders = scope.ServiceProvider.GetServices<IDataSeeder>();
            var dataSeeders = seeders as IDataSeeder[] ?? seeders.ToArray();
            var seedTasks = new List<Task>(dataSeeders.Count());
            seedTasks.AddRange(dataSeeders.OrderBy(o => o.Order).Select(dataSeeder => dataSeeder.SeedAsync()));
            await Task.WhenAll(seedTasks);
            await db.SaveChangesAsync();
        }
    }
}
