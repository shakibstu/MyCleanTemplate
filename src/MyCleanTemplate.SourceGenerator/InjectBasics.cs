using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MyCleanTemplate.SourceGenerator;

[Generator]
public class InjectBasics : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        const string basicDeclarations = """

                                         namespace ServiceGenerator;


                                         using System;
                                         using Microsoft.Extensions.DependencyInjection;

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
                                         
                                         //###########################
                                         [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
                                         internal class InheritableScopedDependencyAttribute<T> : Attribute
                                         {
                                         }
                                         [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
                                         internal class InheritableSingletonDependencyAttribute<T> : Attribute
                                         {
                                         }
                                         [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
                                         internal class InheritableTransientDependencyAttribute<T> : Attribute
                                         {
                                         }
                                         
                                         [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
                                         internal class InheritableSingletonDependencyAttribute : Attribute
                                         {
                                             public InheritableSingletonDependencyAttribute(params Type[] t)
                                             {
                                             }
                                         }
                                         [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
                                         internal class InheritableScopedDependencyAttribute : Attribute
                                         {
                                             public InheritableScopedDependencyAttribute(params Type[] t)
                                             {
                                             }
                                         }
                                         [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
                                         internal class InheritableTransientDependencyAttribute : Attribute
                                         {
                                             public InheritableTransientDependencyAttribute(params Type[] t)
                                             {
                                             }
                                         }
                                         //###########################
                                         internal static partial class SourceGeneratorInjectDependencies
                                         {
                                             public static IServiceCollection InjectFromSourceGenerator(this IServiceCollection services) => services.InjectScoped().InjectSingleton()
                                             .InjectTransient().InjectGenericScoped().InjectGenericSingleton().InjectGenericTransient().InjectScopedInheritable()
                                             .InjectSingletonInheritable().InjectTransientInheritable();
                                             private static partial IServiceCollection InjectScoped(this IServiceCollection services);
                                             private static partial IServiceCollection InjectSingleton(this IServiceCollection services);
                                             private static partial IServiceCollection InjectTransient(this IServiceCollection services);
                                             
                                             private static partial IServiceCollection InjectGenericScoped(this IServiceCollection services);
                                             private static partial IServiceCollection InjectGenericSingleton(this IServiceCollection services);
                                             private static partial IServiceCollection InjectGenericTransient(this IServiceCollection services);
                                         
                                             private static partial IServiceCollection InjectScopedInheritable(this IServiceCollection services);
                                             private static partial IServiceCollection InjectSingletonInheritable(this IServiceCollection services);
                                             private static partial IServiceCollection InjectTransientInheritable(this IServiceCollection services);
                                         }
                                         """;

        context.RegisterPostInitializationOutput(a =>
            a.AddSource("BasicDefinitions.g.cs", SourceText.From(basicDeclarations, Encoding.UTF8)));

    }
}