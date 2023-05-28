using MediatR;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using MyCleanTemplate.Application.Contract.Abstractions;

namespace MyCleanTemplate.Application
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            var assembly = typeof(ApplicationServiceRegistration).Assembly;
            services.AddMediatR(assembly);
            services.InjectSGDependenciesApplication();
            services.AddValidatorsFromAssemblyContaining<IApplicationContractAssemblyMarker>();
            return services;
        }
    }
}
