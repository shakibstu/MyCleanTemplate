using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyCleanTemplate.Domain.Abstractions;
using MyCleanTemplate.Infrastructure.Persistence;
using ServiceGenerator;
namespace MyCleanTemplate.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>((_, options) =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        services.InjectFromSourceGenerator();
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

        var seeders = scope.ServiceProvider.GetServices<IDataSeeder>();
        var dataSeeders = seeders as IReadOnlyList<IDataSeeder> ?? seeders.ToList();
        dataSeeders = dataSeeders.OrderByDescending(o => o.Order).ToList();

        foreach (var seeder in dataSeeders)
        {
            await seeder.SeedAsync();
        }
        await db.SaveChangesAsync();
    }
}