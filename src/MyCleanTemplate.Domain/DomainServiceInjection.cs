using Microsoft.Extensions.DependencyInjection;
using ServiceGenerator;

namespace MyCleanTemplate.Domain;

public static class DomainServiceInjection
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.InjectFromSourceGenerator();
        return services;
    }
}