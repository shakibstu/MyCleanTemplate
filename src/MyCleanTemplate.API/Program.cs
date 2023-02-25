using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace MyCleanTemplate.API;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true, true)
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            //.WriteTo.Console()
            //.WriteTo.File(
            //    new JsonFormatter(), "./Log.json", LogEventLevel.Information)
            .CreateLogger();

        try
        {
            Log.Error("Starting web application...!");
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog();

            var startup = new Startup(builder.Configuration);
            startup.ConfigureServices(builder.Services);
            var app = builder.Build();
            await startup.Configure(app);
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }

    }
}