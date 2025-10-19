using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MyCleanTemplate.SourceGenerator;

[Generator]
public class InjectBasics : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        const string basicDeclarations = $$"""

                                           namespace ServiceGenerator;


                                           using System;
                                           using Microsoft.Extensions.DependencyInjection;
                                           using Castle.DynamicProxy;
                                           
                                           
                                           
                                           
                                           [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
                                           internal partial class InheritableScopedInterceptInterfacesAttribute<T> : Attribute where T : IInterceptor
                                           {
                                           }
                                           
                                           [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
                                           internal partial class InheritableSingletonInterceptInterfacesAttribute<T> : Attribute where T : IInterceptor
                                           {
                                           }
                                           
                                           [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
                                           internal class InheritableTransientInterceptInterfacesAttribute<T> : Attribute where T : IInterceptor
                                           {
                                           }
                                           
                                           
                                           [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
                                           internal partial class InheritableScopedInterceptionAttribute<T> : Attribute where T : IInterceptor
                                           {
                                               public InheritableScopedInterceptionAttribute(params Type[] t)
                                               {
                                               }
                                           }
                                           
                                           [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
                                           internal partial class InheritableSingletonInterceptionAttribute<T> : Attribute where T : IInterceptor
                                           {
                                               public InheritableSingletonInterceptionAttribute(params Type[] t)
                                               {
                                               }
                                           }
                                           
                                           [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
                                           internal class InheritableTransientInterceptionAttribute<T> : Attribute where T : IInterceptor
                                           {
                                               public InheritableTransientInterceptionAttribute(params Type[] t)
                                               {
                                               }
                                           }
                                           
                                           [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
                                           internal partial class ScopedInterceptionAttribute<T> : Attribute where T : IInterceptor
                                           {
                                               public ScopedInterceptionAttribute(params Type[] t)
                                               {
                                               }
                                           }
                                           
                                           [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
                                           internal partial class SingletonInterceptionAttribute<T> : Attribute where T : IInterceptor
                                           {
                                               public SingletonInterceptionAttribute(params Type[] t)
                                               {
                                               }
                                           }
                                           
                                           [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
                                           internal class TransientInterceptionAttribute<T> : Attribute where T : IInterceptor
                                           {
                                               public TransientInterceptionAttribute(params Type[] t)
                                               {
                                               }
                                           }
                                           [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
                                           internal class ScopedDependencyAttribute<T> : Attribute
                                           {
                                           }
                                           [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
                                           internal class SingletonDependencyAttribute<T> : Attribute
                                           {
                                           }
                                           [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
                                           internal class TransientDependencyAttribute<T> : Attribute
                                           {
                                           }

                                           [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
                                           internal class SingletonDependencyAttribute : Attribute
                                           {
                                               public SingletonDependencyAttribute(params Type[] t)
                                               {
                                               }
                                           }
                                           [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
                                           internal class ScopedDependencyAttribute : Attribute
                                           {
                                               public ScopedDependencyAttribute(params Type[] t)
                                               {
                                               }
                                           }
                                           [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
                                           internal class TransientDependencyAttribute : Attribute
                                           {
                                               public TransientDependencyAttribute(params Type[] t)
                                               {
                                               }
                                           }
                                           
                                           [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
                                           internal class InheritableSingletonInterfacesAttribute : Attribute
                                           {
                                           }
                                           [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
                                           internal class InheritableScopedInterfacesAttribute : Attribute
                                           {
                                           }
                                           [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
                                           internal class InheritableTransientInterfacesAttribute : Attribute
                                           {
                                           }
                                           //###########################
                                           [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
                                           internal class InheritableScopedAttribute<T> : Attribute
                                           {
                                           }
                                           [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
                                           internal class InheritableSingletonAttribute<T> : Attribute
                                           {
                                           }
                                           [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
                                           internal class InheritableTransientAttribute<T> : Attribute
                                           {
                                           }
                                           
                                           [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
                                           internal class InheritableSingletonAttribute : Attribute
                                           {
                                               public InheritableSingletonAttribute(params Type[] t)
                                               {
                                               }
                                           }
                                           [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
                                           internal class InheritableScopedAttribute : Attribute
                                           {
                                               public InheritableScopedAttribute(params Type[] t)
                                               {
                                               }
                                           }
                                           [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
                                           internal class InheritableTransientAttribute : Attribute
                                           {
                                               public InheritableTransientAttribute(params Type[] t)
                                               {
                                               }
                                           }
                                           //###########################
                                           internal static partial class SourceGeneratorInjectDependencies
                                           {
                                               public static IServiceCollection InjectFromSourceGenerator(this IServiceCollection services) => services.AddSingleton(new ProxyGenerator()).InjectScoped().InjectSingleton()
                                               .InjectTransient().InjectGenericScoped().InjectGenericSingleton().InjectGenericTransient().InjectScopedInheritable()
                                               .InjectSingletonInheritable().InjectTransientInheritable()
                                               .InjectScopedInterceptionNonInheritable().InjectSingletonInterceptionNonInheritable().InjectTransientInterceptionNonInheritable()
                                               .InjectScopedInterceptionInheritable().InjectSingletonInterceptionInheritable().InjectTransientInterceptionInheritable()
                                               .InjectInheritableScopedInterceptInterfaces().InjectInheritableSingletonInterceptInterfaces().InjectInheritableTransientInterceptInterfaces()
                                               .InjectInheritableScopedWithInterfaces().InjectInheritableSingletonWithInterfaces().InjectInheritableTransientWithInterfaces();
                                               
                                               private static partial IServiceCollection InjectScoped(this IServiceCollection services);
                                               private static partial IServiceCollection InjectSingleton(this IServiceCollection services);
                                               private static partial IServiceCollection InjectTransient(this IServiceCollection services);
                                               
                                               private static partial IServiceCollection InjectGenericScoped(this IServiceCollection services);
                                               private static partial IServiceCollection InjectGenericSingleton(this IServiceCollection services);
                                               private static partial IServiceCollection InjectGenericTransient(this IServiceCollection services);
                                           
                                               private static partial IServiceCollection InjectScopedInheritable(this IServiceCollection services);
                                               private static partial IServiceCollection InjectSingletonInheritable(this IServiceCollection services);
                                               private static partial IServiceCollection InjectTransientInheritable(this IServiceCollection services);
                                               
                                               private static partial IServiceCollection InjectScopedInterceptionNonInheritable(this IServiceCollection services);
                                               private static partial IServiceCollection InjectSingletonInterceptionNonInheritable(this IServiceCollection services);
                                               private static partial IServiceCollection InjectTransientInterceptionNonInheritable(this IServiceCollection services);
                                               
                                               private static partial IServiceCollection InjectScopedInterceptionInheritable(this IServiceCollection services);
                                               private static partial IServiceCollection InjectSingletonInterceptionInheritable(this IServiceCollection services);
                                               private static partial IServiceCollection InjectTransientInterceptionInheritable(this IServiceCollection services);
                                               
                                               private static partial IServiceCollection InjectInheritableScopedInterceptInterfaces(this IServiceCollection services);
                                               private static partial IServiceCollection InjectInheritableSingletonInterceptInterfaces(this IServiceCollection services);
                                               private static partial IServiceCollection InjectInheritableTransientInterceptInterfaces(this IServiceCollection services);
                                               
                                               private static partial IServiceCollection InjectInheritableScopedWithInterfaces(this IServiceCollection services);
                                               private static partial IServiceCollection InjectInheritableSingletonWithInterfaces(this IServiceCollection services);
                                               private static partial IServiceCollection InjectInheritableTransientWithInterfaces(this IServiceCollection services);
                                           }
                                           """;

        context.RegisterPostInitializationOutput(a =>
            a.AddSource("BasicDefinitions.g.cs", SourceText.From(basicDeclarations, Encoding.UTF8)));

    }
}