using MyCleanTemplate.API.Middleware;
using MyCleanTemplate.Application;
using MyCleanTemplate.Domain.Common.Logs;
using MyCleanTemplate.Infrastructure;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ServiceGenerator;
using System.Diagnostics;
using System.Diagnostics.Metrics;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructureServices(builder.Configuration).AddApplicationServices().InjectFromSourceGenerator();
builder.Logging.AddFilter( b => b > LogLevel.Information).AddOpenTelemetry(logging =>

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeScopes = true;
    logging.IncludeFormattedMessage = true;
    logging.ParseStateValues = true;
    logging.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(TelemetryHelper.TelemetryServiceName)).AddOtlpExporter();
});

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics.AddMeter(TelemetryHelper.Meter.Name).AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddOtlpExporter()
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(TelemetryHelper.TelemetryServiceName));
    })
    .WithTracing(b =>
    {
        b.AddSource(TelemetryHelper.TelemetryServiceName)
            .ConfigureResource(resource =>
                resource.AddService(
                    serviceName: TelemetryHelper.TelemetryServiceName).AddAttributes([new KeyValuePair<string, object>("Machine",Environment.MachineName)
                ]))
            .AddEntityFrameworkCoreInstrumentation("SPAAAAAN!!!",a =>
            {
                a.SetDbStatementForText = true;
                a.SetDbStatementForStoredProcedure = true;
            })
            .AddOtlpExporter()
            //.AddAspNetCoreInstrumentation()
            //.AddHttpClientInstrumentation()
            //.AddConsoleExporter()
            //.AddZipkinExporter()
            //.AddOtlpExporter(a => a.Endpoint = new Uri(builder.Configuration.GetConnectionString("Jeager")!))
            ;
    })
    //.UseOtlpExporter()
    ;

builder.Services.AddProblemDetails();
var app = builder.Build();

app.MapDefaultEndpoints();
await app.Services.ConfigureInfrastructure();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
app.UseMiddleware<LogRequestResponseMiddleware>();
app.UseExceptionHandler();
app.UseRouting().UseEndpoints(endpoints => { endpoints.MapControllers(); });
app.UseHttpsRedirection();
app.UseAuthorization();
await app.RunAsync();

namespace MyCleanTemplate.API
{
    public partial class Program { }

}