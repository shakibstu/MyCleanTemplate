using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using MyCleanTemplate.Domain.Abstractions;

namespace MyCleanTemplate.Infrastructure.Helpers
{
    internal static class InjectionHelpers
    {
        internal static IServiceCollection AddRepository<T, TE>(IServiceCollection services) where TE : class, T
            =>
                services.AddScoped(typeof(T), serviceProvider =>
                {
                    var proxyGenerator = serviceProvider.GetRequiredService<ProxyGenerator>();
                    var actual = ActivatorUtilities.CreateInstance<TE>(serviceProvider);
                    ILogRepositoryInterceptor[] interceptors =
                        serviceProvider.GetServices<ILogRepositoryInterceptor>().ToArray();
                    return proxyGenerator.CreateInterfaceProxyWithTarget(typeof(T), actual, interceptors);
                });

    }
}
