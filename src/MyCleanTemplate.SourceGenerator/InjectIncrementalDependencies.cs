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
                builder.Append(
                    $"services.Add{level}(typeof({GetGenericOfAttribute(info.attribute)}),typeof({GetType(info.symbol)}));");
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
    private static string GetGenericOfAttribute(AttributeData attribute) =>
        attribute.AttributeClass!.TypeArguments[0].OriginalDefinition.ToString();

    private static IEnumerable<string> GetTypeOfParametricAttribute(AttributeData attribute)
    {
        if( attribute.ConstructorArguments[0].Values is IEnumerable<TypedConstant> types)
        {
            return types.Select(t => t.Value!.ToString());
        }
        return [];
    }
}