using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;
using MyCleanTemplate.Application.Contract.Abstractions.AssemblyMarker;

namespace MyCleanTemplate.Application
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.InjectSGDependenciesApplication();
            services.AddValidatorsFromAssemblyContaining(typeof(IApplicationContractAssemblyMarker));
            return services;
        }
    }
}
