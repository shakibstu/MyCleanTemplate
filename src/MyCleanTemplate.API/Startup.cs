using System.Data;
using Microsoft.EntityFrameworkCore;
using MyCleanTemplate.Infrastructure;
using MyCleanTemplate.Infrastructure.Persistence;

namespace MyCleanTemplate.API
{

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddAutoMapper(typeof(Startup));
            services.AddInfrastructureServices(Configuration);
        }
        public async Task Configure(WebApplication app)
        {
            var scope = app.Services.CreateAsyncScope();

            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if ((await db.Database.GetPendingMigrationsAsync()).Any())
            {
                await db.Database.MigrateAsync();
            }

            var seeders = scope.ServiceProvider.GetServices<IDataSeeder>();

            var dataSeeders = seeders as IDataSeeder[] ?? seeders.ToArray();
            var seedTasks = new List<Task>(dataSeeders.Count());
            foreach (var dataSeeder in dataSeeders)
            {
                seedTasks.Add(dataSeeder.SeedAsync());
            }
            await Task.WhenAll(seedTasks);
            await db.SaveChangesAsync();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseExceptionHandler("/api/error");
            app.UseRouting();
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
