using MyCleanTemplate.Common.Logs;
using Serilog;

namespace MyCleanTemplate.API;

public static class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog(
                (context, configuration) =>
                {
                    configuration.ReadFrom.Configuration(context.Configuration);
                    configuration.Filter.ByExcluding(a =>
                    {
                        a.Properties.TryGetValue(nameof(LogKeywords.SourceContext), out var value);
                        return value?.ToString().Replace("\"", "") != LogKeywords.SourceContext;
                    });
                });

            var startup = new Startup(builder.Configuration);
            startup.ConfigureServices(builder.Services);
            var app = builder.Build();
            await Startup.Configure(app);
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }

    }
}