using Microsoft.AspNetCore.Builder;
using MyCleanTemplate.API.Middleware;
using MyCleanTemplate.Application;
using MyCleanTemplate.Infrastructure;
using Serilog;

namespace MyCleanTemplate.API
{

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddAutoMapper(typeof(Startup));
            services.AddInfrastructureServices(Configuration).AddApplicationServices().InjectSGDependenciesAPI()
                .AddTransient<AddRequestIdToLogContextMiddleware>().AddTransient<LogRequestResponseMiddleware>();
        }

        public static async Task Configure(WebApplication app)
        {
            await app.Services.ConfigureInfrastructure();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
            }

            app.UseExceptionHandler("/api/error");
            app.UseRouting();
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.UseMiddleware<AddRequestIdToLogContextMiddleware>();
            app.UseMiddleware<LogRequestResponseMiddleware>();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
