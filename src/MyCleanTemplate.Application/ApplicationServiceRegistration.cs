using MediatR;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;

namespace MyCleanTemplate.Application
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            var assembly = typeof(ApplicationServiceRegistration).Assembly;
            services.AddAutoMapper(assembly);
            services.AddMediatR(assembly);
            services.InjectSGDependenciesApplication();
            services.AddValidatorsFromAssembly(assembly);
            return services;
        }
    }
}
