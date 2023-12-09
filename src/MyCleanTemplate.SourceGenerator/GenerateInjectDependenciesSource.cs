using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace MyCleanTemplate.SourceGenerator
{
    [Generator]
    public class GenerateInjectDependenciesSource : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new DiReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is DiReceiver diReceiver)
            {
                var di = $@"  
    namespace ServiceGenerator;
    using Microsoft.Extensions.DependencyInjection;
    public static class DependencyInjectionHelper
    {{
        public static IServiceCollection InjectSGDependencies{context.Compilation.AssemblyName!.Split('.').Last()}(this IServiceCollection services){{
            {string.Join("\n", diReceiver.Injections)}
            return services;
        }}
    }}
";
                context.AddSource("DependencyInjectionHelper_sg.cs", di);
            }
        }
    }


    public class DiReceiver : ISyntaxContextReceiver
    {
        public readonly List<string> Injections = new();
        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            var syntaxNode = context.Node;
            var model = context.SemanticModel;
            if (syntaxNode is not ClassDeclarationSyntax classDeclaration) return;

            var symbol =
                model.GetDeclaredSymbol(classDeclaration);
            if (symbol == null) return;

            var scopeDependencyFullName = "ServiceGenerator.IScopeDependency";

            var singletonDependencyFullName = "ServiceGenerator.ISingletonDependency";

            var transientDependencyFullName = "ServiceGenerator.ITransientDependency";

            var scopeJustImplementationDependencyFullName = "ServiceGenerator.IScopeJustImplementationDependency";

            var singletonJustImplementationDependencyFullName = "ServiceGenerator.ISingletonJustImplementationDependency";

            var transientJustImplementationDependencyFullName = "ServiceGenerator.ITransientJustImplementationDependency";

            var repositoryAttributeFullName = "ServiceGenerator.RepositoryAttribute";
            var attributes = symbol.GetAttributes();
            if (attributes.Any(a => GetType(a.AttributeClass!) == repositoryAttributeFullName))
            {
                Injections.AddRange(
                    symbol.Interfaces.Where(a => a.Name == $"I{symbol.Name}")
                        .Select(a =>
                            $"InjectionHelpers.AddRepository<{GetType(a)},{GetType(symbol.OriginalDefinition)}>(services);"));
            }
            switch (symbol.Interfaces)
            {
                case var _ when symbol.Interfaces.Any(a => GetType(a) == scopeDependencyFullName):
                    Injections.AddRange(
                        symbol.Interfaces.Where(a => GetType(a) != scopeDependencyFullName)
                            .Select(a =>
                                $"services.AddScoped(typeof({GetType(a)}),typeof({GetType(symbol.OriginalDefinition)}));"));
                    if (symbol.BaseType is { IsAbstract: true, DeclaredAccessibility: Accessibility.Public })
                    {
                        Injections.Add(
                                    $"services.AddScoped(typeof({GetType(symbol.BaseType)}),typeof({GetType(symbol.OriginalDefinition)}));");
                    }
                    break;

                case var _ when symbol.Interfaces.Any(a => GetType(a) == singletonDependencyFullName):
                    Injections.AddRange(
                        symbol.Interfaces.Where(a => GetType(a) != singletonDependencyFullName)
                            .Select(a =>
                                $"services.AddSingleton(typeof({GetType(a)}),typeof({GetType(symbol.OriginalDefinition)}));"));
                    if (symbol.BaseType is { IsAbstract: true, DeclaredAccessibility: Accessibility.Public })
                    {
                        Injections.Add(
                            $"services.AddSingleton(typeof({GetType(symbol.BaseType)}),typeof({GetType(symbol.OriginalDefinition)}));");
                    }
                    break;

                case var _ when symbol.AllInterfaces.Any(a => GetType(a) == transientDependencyFullName):
                    Injections.AddRange(
                        symbol.Interfaces.Where(a => GetType(a) != transientDependencyFullName)
                            .Select(a =>
                                $"services.AddTransient(typeof({GetType(a)}),typeof({GetType(symbol.OriginalDefinition)}));"));
                    if (symbol.BaseType is { IsAbstract: true, DeclaredAccessibility: Accessibility.Public })
                    {
                        Injections.Add(
                            $"services.AddTransient(typeof({GetType(symbol.BaseType)}),typeof({GetType(symbol.OriginalDefinition)}));");
                    }
                    break;

                case var _ when symbol.Interfaces.Any(a => GetType(a) == scopeJustImplementationDependencyFullName):
                    Injections.Add($"services.AddScoped(typeof({GetType(symbol.OriginalDefinition)}));");
                    break;

                case var _ when symbol.Interfaces.Any(a => GetType(a) == singletonJustImplementationDependencyFullName):
                    Injections.Add($"services.AddSingleton(typeof({GetType(symbol.OriginalDefinition)}));");
                    break;

                case var _ when symbol.AllInterfaces.Any(a => GetType(a) == transientJustImplementationDependencyFullName):
                    Injections.Add($"services.AddTransient(typeof({GetType(symbol.OriginalDefinition)}));");
                    break;
            }
        }

        private string GetType(INamedTypeSymbol symbol)
        {
            return symbol.IsGenericType ? symbol.ConstructUnboundGenericType().ToString() : symbol.OriginalDefinition.ToString();
        }
    }

}
