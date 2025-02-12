using MyCleanTemplate.API.Middleware;
using MyCleanTemplate.Application;
using MyCleanTemplate.Domain.Common.Logs;
using MyCleanTemplate.Infrastructure;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ServiceGenerator;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructureServices(builder.Configuration).AddApplicationServices().InjectFromSourceGenerator();
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
});

builder.Services.AddOpenTelemetry()
    //.WithMetrics(metrics =>
    //{
    //    metrics.AddAspNetCoreInstrumentation()
    //        .AddHttpClientInstrumentation()
    //        .AddRuntimeInstrumentation();
    //})
    .WithTracing(b =>
    {
        b.AddSource(LogKeywords.TelemetryServiceName)
            .ConfigureResource(resource =>
                resource.AddService(
                    serviceName: LogKeywords.TelemetryServiceName))
            .AddAspNetCoreInstrumentation()
            //.AddConsoleExporter()
            //.AddZipkinExporter()
        //.AddOtlpExporter(a => a.Endpoint = new Uri(builder.Configuration.GetConnectionString("Jeager")!))
            ;
    })
    .UseOtlpExporter()
    ;

var app = builder.Build();

app.MapDefaultEndpoints();
await app.Services.ConfigureInfrastructure();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseExceptionHandler("/api/error");
app.UseMiddleware<LogRequestResponseMiddleware>();
app.UseRouting().UseEndpoints(endpoints => { endpoints.MapControllers(); });
app.UseHttpsRedirection();
app.UseAuthorization();
await app.RunAsync();

namespace MyCleanTemplate.API
{
    public partial class Program { }
}