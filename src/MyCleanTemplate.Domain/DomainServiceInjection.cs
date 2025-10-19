using Microsoft.Extensions.DependencyInjection;
using MyCleanTemplate.Domain.Abstractions.Messaging;
using MyCleanTemplate.Domain.Common.Messaging;
using ServiceGenerator;

namespace MyCleanTemplate.Domain
{
    public static class DomainServiceInjection
    {
        public static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            services.AddScoped<IMediator,Mediator>().InjectFromSourceGenerator();
            return services;
        }
    }
}
