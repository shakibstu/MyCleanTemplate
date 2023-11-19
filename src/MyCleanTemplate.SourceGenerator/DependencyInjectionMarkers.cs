using Microsoft.CodeAnalysis;

namespace MyCleanTemplate.SourceGenerator
{
    [Generator]
    public class DependencyInjectionMarker : ISourceGenerator
    {


        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new DiMarker());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is DiMarker diMarker && context.Compilation.AssemblyName!.EndsWith("Domain"))
            {
                context.AddSource("DependencyInjectionMarker_sg.cs", diMarker.DiMarkers);
            }
        }

        private class DiMarker : ISyntaxContextReceiver
        {
            public readonly string DiMarkers = """
                                               namespace ServiceGenerator{
                                               using System;
                                               using Castle.DynamicProxy;
                                               using Microsoft.Extensions.DependencyInjection;

                                               public interface IScopeDependency{}
                                               public interface IScopeJustImplementationDependency{}
                                               public interface ISingletonDependency{}
                                               public interface ISingletonJustImplementationDependency{}
                                               public interface ITransientDependency{}
                                               public interface ITransientJustImplementationDependency{}
                                               public class RepositoryAttribute : Attribute{}
                                               public interface ILogRepositoryInterceptor : IInterceptor{}
                                               
                                               
                                               public static class InjectionHelpers
                                               {
                                                   public static IServiceCollection AddRepository<T, TE>(IServiceCollection services) where TE : class, T
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
                                               """;
            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            { }
        }
    }
}
