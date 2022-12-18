using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using MediatR;

namespace MyCleanTemplate.Application
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddMediatR(Assembly.GetExecutingAssembly());

            services.Scan(scan => scan.FromCallingAssembly().AddClasses(classes => classes.AssignableTo(typeof(IPipelineBehavior<,>)))
            .AsImplementedInterfaces()
            .WithTransientLifetime());

            return services;
        }
    }
}
