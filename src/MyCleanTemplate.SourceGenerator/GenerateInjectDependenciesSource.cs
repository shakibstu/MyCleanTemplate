
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MyCleanTemplate.Common.Dependencies;
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
    namespace {context.Compilation.AssemblyName};
    using Microsoft.Extensions.DependencyInjection;
    public static class DependencyInjectionHelper
    {{
        public static IServiceCollection InjectSGDependencies{context.Compilation.AssemblyName!.Split('.').Last()}(this IServiceCollection services){{
            {string.Join("\n", diReceiver.Injections)}
            return services;
        }}
    }}";
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

            var scopeDependencyMetadata =
                model.Compilation.GetTypeByMetadataName(typeof(IScopeDependency).FullName);

            var singletonDependencyMetadata =
                model.Compilation.GetTypeByMetadataName(typeof(ISingletonDependency).FullName);

            var transientDependencyMetadata =
                model.Compilation.GetTypeByMetadataName(typeof(ITransientDependency).FullName);

            var scopeJustImplementationDependencyMetadata =
                model.Compilation.GetTypeByMetadataName(typeof(IScopeJustImplementationDependency).FullName);

            var singletonJustImplementationDependencyMetadata =
                model.Compilation.GetTypeByMetadataName(typeof(ISingletonJustImplementationDependency).FullName);

            var transientJustImplementationDependencyMetadata =
                model.Compilation.GetTypeByMetadataName(typeof(ITransientJustImplementationDependency).FullName);

            var repositoryAttribute = model.Compilation.GetTypeByMetadataName(typeof(RepositoryAttribute).FullName);
            var attributes = symbol.GetAttributes();
            if (attributes.Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, repositoryAttribute)))
            {
                Injections.AddRange(
                    symbol.Interfaces.Where(a => a.Name == $"I{symbol.Name}")
                        .Select(a =>
                            $"MyCleanTemplate.Infrastructure.Helpers.InjectionHelpers.AddRepository<{GetType(a)},{GetType(symbol.OriginalDefinition)}>(services);"));
            }
            switch (symbol.Interfaces)
            {
                case var _ when symbol.Interfaces.Any(a => SymbolEqualityComparer.Default.Equals(a, scopeDependencyMetadata)):
                    Injections.AddRange(
                        symbol.Interfaces.Where(a => !SymbolEqualityComparer.Default.Equals(a, scopeDependencyMetadata))
                            .Select(a =>
                                $"services.AddScoped(typeof({GetType(a)}),typeof({GetType(symbol.OriginalDefinition)}));"));
                    if (symbol.BaseType is { IsAbstract: true, DeclaredAccessibility: Accessibility.Public })
                    {
                        Injections.Add(
                                    $"services.AddScoped(typeof({GetType(symbol.BaseType)}),typeof({GetType(symbol.OriginalDefinition)}));");
                    }
                    break;

                case var _ when symbol.Interfaces.Any(a => SymbolEqualityComparer.Default.Equals(a, singletonDependencyMetadata)):
                    Injections.AddRange(
                        symbol.Interfaces.Where(a => !SymbolEqualityComparer.Default.Equals(a, singletonDependencyMetadata))
                            .Select(a =>
                                $"services.AddSingleton(typeof({GetType(a)}),typeof({GetType(symbol.OriginalDefinition)}));"));
                    if (symbol.BaseType is { IsAbstract: true, DeclaredAccessibility: Accessibility.Public })
                    {
                        Injections.Add(
                            $"services.AddSingleton(typeof({GetType(symbol.BaseType)}),typeof({GetType(symbol.OriginalDefinition)}));");
                    }
                    break;

                case var _ when symbol.AllInterfaces.Any(a => SymbolEqualityComparer.Default.Equals(a, transientDependencyMetadata)):
                    Injections.AddRange(
                        symbol.Interfaces.Where(a => !SymbolEqualityComparer.Default.Equals(a, transientDependencyMetadata))
                            .Select(a =>
                                $"services.AddTransient(typeof({GetType(a)}),typeof({GetType(symbol.OriginalDefinition)}));"));
                    if (symbol.BaseType is { IsAbstract: true, DeclaredAccessibility: Accessibility.Public })
                    {
                        Injections.Add(
                            $"services.AddTransient(typeof({GetType(symbol.BaseType)}),typeof({GetType(symbol.OriginalDefinition)}));");
                    }
                    break;

                case var _ when symbol.Interfaces.Any(a => SymbolEqualityComparer.Default.Equals(a, scopeJustImplementationDependencyMetadata)):
                    Injections.Add($"services.AddScoped(typeof({GetType(symbol.OriginalDefinition)}));");
                    break;

                case var _ when symbol.Interfaces.Any(a => SymbolEqualityComparer.Default.Equals(a, singletonJustImplementationDependencyMetadata)):
                    Injections.Add($"services.AddSingleton(typeof({GetType(symbol.OriginalDefinition)}));");
                    break;

                case var _ when symbol.AllInterfaces.Any(a => SymbolEqualityComparer.Default.Equals(a, transientJustImplementationDependencyMetadata)):
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
