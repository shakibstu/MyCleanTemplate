using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyCleanTemplate.API;
using MyCleanTemplate.Infrastructure.Persistence;

namespace MyCleanTemplate.IntegrationTest.Automation
{
    public abstract class BaseIntegrationTest : WebApplicationFactory<Program>
    {
        protected readonly HttpClient HttpClient;
        
        protected IServiceProvider ServiceProvider => Scope.ServiceProvider;

        protected readonly IServiceScope Scope;
        protected BaseIntegrationTest()
        {
            HttpClient = CreateClient();
            Scope = Services.CreateScope();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite(connection));
            });
        }

        protected override void Dispose(bool disposing)
        {
            HttpClient.Dispose();
            Scope.Dispose();
            base.Dispose(disposing);
        }
        ~BaseIntegrationTest()
        {
            Dispose(false);
        }
    }
}
