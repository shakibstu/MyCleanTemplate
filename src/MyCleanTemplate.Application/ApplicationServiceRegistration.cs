using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MyCleanTemplate.Application.Contract.Abstractions;

namespace MyCleanTemplate.Application
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            var assembly = typeof(ApplicationServiceRegistration).Assembly;
            services.AddMediatR(a => a.RegisterServicesFromAssembly(assembly));
            services.InjectSGDependenciesApplication();
            services.AddValidatorsFromAssemblyContaining<IApplicationContractAssemblyMarker>();
            return services;
        }
    }
}
