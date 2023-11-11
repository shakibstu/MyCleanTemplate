using MyCleanTemplate.API;
using MyCleanTemplate.API.Middleware;
using MyCleanTemplate.Application;
using MyCleanTemplate.Common.Logs;
using MyCleanTemplate.Infrastructure;
using Serilog;


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
builder.Services.AddInfrastructureServices(builder.Configuration).AddApplicationServices().InjectSGDependenciesAPI();
var app = builder.Build();
await app.Services.ConfigureInfrastructure();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseRouting().UseEndpoints(endpoints => { endpoints.MapControllers(); });
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseMiddleware<AddRequestIdToLogContextMiddleware>();
app.UseMiddleware<LogRequestResponseMiddleware>();
app.UseExceptionHandler("/api/error");
await app.RunAsync();
public partial class Program { }