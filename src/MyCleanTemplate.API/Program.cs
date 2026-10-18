using MyCleanTemplate.API.Errors;
using MyCleanTemplate.API.Middleware;
using MyCleanTemplate.Application;
using MyCleanTemplate.Domain;
using MyCleanTemplate.Domain.Common.Logs;
using MyCleanTemplate.Infrastructure;
using MyCleanTemplate.Infrastructure.Persistence;
using MyCleanTemplate.ServiceDefaults;
using Microsoft.OpenApi;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ServiceGenerator;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer", doc), [] }
    });
});
builder.Services.AddInfrastructureServices(builder.Configuration).AddApplicationServices().AddDomainServices().InjectFromSourceGenerator();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database", tags: ["ready"]);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Logging.AddFilter(b => b > LogLevel.Information).AddOpenTelemetry(logging =>
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
                    serviceName: TelemetryHelper.TelemetryServiceName).AddAttributes([
                    new KeyValuePair<string, object>("Machine", Environment.MachineName)
                ]))
            .AddEntityFrameworkCoreInstrumentation()
            .AddOtlpExporter();
    });

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
app.UseHttpsRedirection();

// Correct middleware order: Routing -> Authentication -> Authorization -> Endpoints
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();

namespace MyCleanTemplate.API
{
    public partial class Program;

}