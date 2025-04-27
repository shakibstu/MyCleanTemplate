using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyCleanTemplate.API;
using MyCleanTemplate.Infrastructure.Persistence;
using Xunit;

namespace MyCleanTemplate.IntegrationTest.Automation;

public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
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
        base.Dispose(disposing);
    }

    ~IntegrationTestWebApplicationFactory()
    {
        Dispose(false);
    }

    public ValueTask InitializeAsync()
    {
        return ValueTask.CompletedTask;
        //throw new NotImplementedException();
    }

    public new Task DisposeAsync()
    {
        return Task.CompletedTask;
        //throw new NotImplementedException();
    }
}