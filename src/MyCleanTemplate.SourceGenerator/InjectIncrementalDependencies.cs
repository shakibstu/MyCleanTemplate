using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace MyCleanTemplate.SourceGenerator;

public static class InjectIncrementalDependencies
{
    public static void InitializeWithLevel(IncrementalGeneratorInitializationContext context, string level)
    {
        InitializeWithLevelWithoutGeneric(context, level);
        InitializeGenericWithLevel(context, level);
        InitializeGenericWithInherit(context, level);
    }
    private static bool IsClassDeclaration(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax;
    }

    private static (SemanticModel SemanticModel, ClassDeclarationSyntax ClassSyntax) GetClassDeclarationSyntax(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        return (context.SemanticModel, classDeclaration);
    }

    private static List<string> InheritableInjections(SemanticModel semanticModel, ClassDeclarationSyntax classDeclaration, string attributeName)
    {
        var types = new List<string>();
        // Get the symbol for the current class
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
        if (classSymbol == null)
            return types;

        // Check interfaces of class
        var interfaceAttributes = classSymbol.AllInterfaces.SelectMany(a => a.GetAttributes())
            .Where(b => GetType(b.AttributeClass!) == attributeName).ToList();
        
        if (interfaceAttributes.Any())
        {
            types.AddRange(interfaceAttributes.SelectMany(GetTypeOfParametricAttribute));
        }

        var tmpSymbol = classSymbol;

        while (tmpSymbol != null)
        {
            var attributes = tmpSymbol.GetAttributes()
                .Where(attr => GetType(attr.AttributeClass!) == attributeName).ToList();
            if (attributes.Any())
            {
                types.AddRange(attributes.SelectMany(GetTypeOfParametricAttribute));
            }
            tmpSymbol = tmpSymbol.BaseType;
        }

        return types;
    }

    private static void InitializeGenericWithInherit(IncrementalGeneratorInitializationContext context,
        string level)
    {
        var template = new StringBuilder(@"
                             namespace ServiceGenerator;
                             using Microsoft.Extensions.DependencyInjection;
                             internal static partial class SourceGeneratorInjectDependencies
                             {{
                                 private static partial IServiceCollection Inject" + level + @"Inheritable(this IServiceCollection services)
                                 {{ 
                                    {0}
                                    return services;
                                 }}
                             }}");
        // Step 1: Identify all class declarations
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsClassDeclaration(node),
                transform: static (context, _) => GetClassDeclarationSyntax(context));

        // Step 2: Combine syntax and semantic model
        var classDeclarationsWithSemanticModel = classDeclarations
            .SelectMany((syntax, _) =>
            {
                var semanticModel = syntax.SemanticModel;
                var classDeclaration = syntax.ClassSyntax;
                var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
                var injections = InheritableInjections(semanticModel, classDeclaration, $"ServiceGenerator.Inheritable{level}DependencyAttribute");
                injections.AddRange(InheritableInjections(semanticModel, classDeclaration, $"ServiceGenerator.Inheritable{level}DependencyAttribute<>"));

                return injections.Select(injection => $"services.Add{level}(typeof({injection}),typeof({GetType(classSymbol!)}));");
            }).Collect();
        // Step 3: Check for the parent class attribute
        context.RegisterSourceOutput(classDeclarationsWithSemanticModel, (ctx, injections) =>
        {
            var injected = string.Format(template.ToString(), string.Join("\n", injections));
            ctx.AddSource($"{level}.Inheritable.cs", injected);
        });
    }

    private static void InitializeWithLevelWithoutGeneric(IncrementalGeneratorInitializationContext context , string level)
    {
        var template = new StringBuilder(@"
                             namespace ServiceGenerator;
                             using Microsoft.Extensions.DependencyInjection;
                             internal static partial class SourceGeneratorInjectDependencies
                             {{
                                 private static partial IServiceCollection Inject"+ level + @"(this IServiceCollection services)
                                 {{ 
                                    {0}
                                    return services;
                                 }}
                             }}");
        var provider = context.SyntaxProvider.ForAttributeWithMetadataName($"ServiceGenerator.{level}DependencyAttribute`1",
            (node, _) => node is ClassDeclarationSyntax, (syntaxContext, _) =>
            {
                var classDeclaration = (ClassDeclarationSyntax)syntaxContext.TargetNode;
                var classSymbol = (INamedTypeSymbol)syntaxContext.TargetSymbol;
                return (classDeclaration, classSymbol, syntaxContext.Attributes);
            }).SelectMany((info, _) =>
        {
            var array = ImmutableArray
                .CreateBuilder<(ClassDeclarationSyntax node, INamedTypeSymbol symbol, AttributeData attribute)>(
                    info.Attributes.Length);
            foreach (var attribute in info.Attributes)
            {
                array.Add((info.classDeclaration, info.classSymbol, attribute));
            }

            return array.ToImmutable();
        }).Collect();

        context.RegisterSourceOutput(provider, (a, infos) =>
        {

            StringBuilder builder = new StringBuilder();

            foreach (var info in infos)
            {
                builder.AppendLine();
                var injections = GetTypeOfParametricAttribute(info.attribute);
                foreach (var injection in injections)
                {

                    builder.Append(
                        $"services.Add{level}(typeof({injection}),typeof({GetType(info.symbol)}));");
                    builder.AppendLine();
                }
            }

            var injected = string.Format(template.ToString(), builder);
            a.AddSource($"{level}.g.cs", injected);
        });

    }
    private static string GetType(INamedTypeSymbol symbol)
    {
        return symbol.IsGenericType ? symbol.ConstructUnboundGenericType().ToString() : symbol.OriginalDefinition.ToString();
    }

    private static void InitializeGenericWithLevel(IncrementalGeneratorInitializationContext context, string level)
    {
        var template = new StringBuilder(@"
                             namespace ServiceGenerator;
                             using Microsoft.Extensions.DependencyInjection;
                             internal static partial class SourceGeneratorInjectDependencies
                             {{
                                 private static partial IServiceCollection InjectGeneric" + level + @"(this IServiceCollection services)
                                 {{ 
                                    {0}
                                    return services;
                                 }}
                             }}");


        var provider = context.SyntaxProvider.ForAttributeWithMetadataName($"ServiceGenerator.{level}DependencyAttribute",
            (node, _) => node is ClassDeclarationSyntax, (syntaxContext, _) =>
            {
                var classDeclaration = (ClassDeclarationSyntax)syntaxContext.TargetNode;
                var classSymbol = (INamedTypeSymbol)syntaxContext.TargetSymbol;
                return (classDeclaration, classSymbol, syntaxContext.Attributes);
            }).SelectMany((info, _) =>
        {
            var array = ImmutableArray
                .CreateBuilder<(ClassDeclarationSyntax node, INamedTypeSymbol symbol, AttributeData attribute)>(
                    info.Attributes.Length);
            foreach (var attribute in info.Attributes)
            {
                array.Add((info.classDeclaration, info.classSymbol, attribute));
            }

            return array.ToImmutable();
        }).Collect();
        
        context.RegisterSourceOutput(provider, (a, infos) =>
        {

            StringBuilder builder = new StringBuilder();

            foreach (var info in infos)
            {

                builder.AppendLine();
                var injections = GetTypeOfParametricAttribute(info.attribute);
                foreach (var injection in injections)
                {

                    builder.Append(
                        $"services.Add{level}(typeof({injection}),typeof({GetType(info.symbol)}));");
                    builder.AppendLine();
                }
            }

            var injected = string.Format(template.ToString(), builder);
            a.AddSource($"{level}.Generic.g.cs", injected);
        });

    }
    private static IEnumerable<string> GetTypeOfParametricAttribute(AttributeData attribute)
    {
        if(attribute.AttributeClass!.IsGenericType)
        {
            return attribute.AttributeClass!.TypeArguments.Select(t => t.OriginalDefinition.ToString());
        }
        if ( attribute.ConstructorArguments[0].Values is IEnumerable<TypedConstant> types)
        {
            return types.Select(t => t.Value!.ToString());
        }
        return [];
    }
}