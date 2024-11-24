using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MyCleanTemplate.Application.Common.Behaviors;
using MyCleanTemplate.Application.Contract.Abstractions;
using ServiceGenerator;

namespace MyCleanTemplate.Application
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            var assembly = typeof(ApplicationServiceRegistration).Assembly;
            services.AddMediatR(a => a.RegisterServicesFromAssembly(assembly));
            //services.InjectSGDependenciesApplication();
            services.InjectFromSourceGenerator();
            services.AddValidatorsFromAssemblyContaining<IApplicationContractAssemblyMarker>();
            return services;
        }
    }
}
