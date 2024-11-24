using MyCleanTemplate.API.Middleware;
using MyCleanTemplate.Application;
using MyCleanTemplate.Domain.Common.Logs;
using MyCleanTemplate.Infrastructure;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using ServiceGenerator;

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
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructureServices(builder.Configuration).AddApplicationServices().InjectFromSourceGenerator();

builder.Services.AddOpenTelemetry()
    .WithTracing(b =>
    {
        b.AddSource(LogKeywords.TelemetryServiceName)
            .ConfigureResource(resource =>
                resource.AddService(
                    serviceName: LogKeywords.TelemetryServiceName))
            .AddAspNetCoreInstrumentation()
            .AddConsoleExporter()
            .AddZipkinExporter()
            .AddOtlpExporter(a => a.Endpoint = new Uri(builder.Configuration.GetConnectionString("Jeager")!));
    });

var app = builder.Build();
await app.Services.ConfigureInfrastructure();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseExceptionHandler("/api/error");
app.UseMiddleware<AddRequestIdToLogContextMiddleware>();
app.UseMiddleware<LogRequestResponseMiddleware>();
app.UseRouting().UseEndpoints(endpoints => { endpoints.MapControllers(); });
app.UseHttpsRedirection();
app.UseAuthorization();
await app.RunAsync();

namespace MyCleanTemplate.API
{
    public partial class Program { }
}