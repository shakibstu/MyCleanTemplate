using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MyCleanTemplate.SourceGenerator
{
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
                                                 public SingletonDependencyAttribute(Type t)
                                                 {
                                                 }
                                             }
                                             [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
                                             internal class ScopedDependencyAttribute : Attribute
                                             {
                                                 public ScopedDependencyAttribute(Type t)
                                                 {
                                                 }
                                             }
                                             [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
                                             internal class TransientDependencyAttribute : Attribute
                                             {
                                                 public TransientDependencyAttribute(Type t)
                                                 {
                                                 }
                                             }
                                             
                                             internal static partial class SourceGeneratorInjectDependencies
                                             {
                                                 public static IServiceCollection InjectFromSourceGenerator(this IServiceCollection services) => services.InjectScoped().InjectSingleton().InjectTransient();
                                                 private static partial IServiceCollection InjectScoped(this IServiceCollection services);
                                                 private static partial IServiceCollection InjectSingleton(this IServiceCollection services);
                                                 private static partial IServiceCollection InjectTransient(this IServiceCollection services);
                                                 
                                                 private static partial IServiceCollection InjectGenericScoped(this IServiceCollection services);
                                                 private static partial IServiceCollection InjectGenericSingleton(this IServiceCollection services);
                                                 private static partial IServiceCollection InjectGenericTransient(this IServiceCollection services);
                                             }
                                             """;

            context.RegisterPostInitializationOutput(a =>
                a.AddSource("BasicDefinitions.g.cs", SourceText.From(basicDeclarations, Encoding.UTF8)));

        }
    }
}
