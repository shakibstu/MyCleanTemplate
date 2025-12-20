
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Text;
namespace MyCleanTemplate.SourceGenerator;

public static class InjectIncrementalDependencies
{
    public static void InitializeWithLevel(IncrementalGeneratorInitializationContext context, string level)
    {
        InjectSimpleDependenciesForTypesInGenericAttribute(context, level);
        InjectSimpleDependenciesForTypesInAttribute(context, level);
        InjectInheritableDependenciesForTypesInGenericAttribute(context, level);
        InjectSimpleDependenciesWithInterceptionInGenericAttribute(context, level);
        InjectInheritableDependenciesWithInterceptionInGenericAttribute(context, level);
        InitializeWithInterceptionInheritableInjectInterfaces(context, level);
        InjectInheritableWithInterfaces(context, level);
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

    private static List<(string generic, List<string> types)> InheritableInjections(SemanticModel semanticModel, ClassDeclarationSyntax classDeclaration, string attributeName)
    {
        var types = new List<(string generic, List<string> types)>();
        // Get the symbol for the current class
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
        if (classSymbol == null)
            return types;

        // Check interfaces of class
        var interfaceAttributes = classSymbol.AllInterfaces.SelectMany(a => a.GetAttributes())
            .Where(b => GetType(b.AttributeClass!, false) == attributeName).ToList();

        if (interfaceAttributes.Any())
        {
            types.AddRange(interfaceAttributes.Select(GetTypeOfParametricAttribute));
        }

        var tmpSymbol = classSymbol.BaseType;

        while (tmpSymbol != null)
        {
            var attributes = tmpSymbol.GetAttributes()
                .Where(attr => GetType(attr.AttributeClass!, false) == attributeName).ToList();
            if (attributes.Any())
            {
                types.AddRange(attributes.Select(GetTypeOfParametricAttribute));
            }
            tmpSymbol = tmpSymbol.BaseType;
        }

        return types;
    }

    private static void InjectInheritableDependenciesForTypesInGenericAttribute(IncrementalGeneratorInitializationContext context,
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
                var injections = InheritableInjections(semanticModel, classDeclaration, $"ServiceGenerator.Inheritable{level}Attribute");
                injections.AddRange(InheritableInjections(semanticModel, classDeclaration, $"ServiceGenerator.Inheritable{level}Attribute<>"));

                var finalInjections = injections.SelectMany(MergeString);

                return finalInjections.Select(injection => $"services.Add{level}(typeof({injection}),typeof({GetType(classSymbol!, true)}));");
            }).Collect();
        // Step 3: Check for the parent class attribute
        context.RegisterSourceOutput(classDeclarationsWithSemanticModel, (ctx, injections) =>
        {
            var injected = string.Format(template.ToString(), string.Join("\n", injections));
            ctx.AddSource($"{level}.Inheritable.cs", injected);
        });
    }

    private static string GetInterceptionHelper(string level)
    {
        return @"
    private static IServiceCollection " + level + @"InterceptWithTarget(this IServiceCollection services,
        Type interfaceType, Type interceptorType, Type classType)
    {{
        return services.Add" + level + @"(interfaceType, serviceProvider =>
        {{
            {{
                var proxyGenerator = serviceProvider.GetRequiredService<ProxyGenerator>();
                var actual = ActivatorUtilities.CreateInstance(serviceProvider,classType);
                var interceptor = (IInterceptor) ActivatorUtilities.CreateInstance(serviceProvider, interceptorType);
                return proxyGenerator.CreateInterfaceProxyWithTarget(interfaceType, actual, interceptor);
            }}
        }});
    }}
            
    private static IServiceCollection " + level + @"InterceptWithoutTarget(this IServiceCollection services,
        Type interfaceType, Type interceptorType)
    {{
        return services.Add" + level + @"(interfaceType, serviceProvider =>
        {{
            {{
                var proxyGenerator = serviceProvider.GetRequiredService<ProxyGenerator>();
                var interceptor = (IInterceptor)ActivatorUtilities.CreateInstance(serviceProvider, interceptorType);
                return proxyGenerator.CreateInterfaceProxyWithoutTarget(interfaceType, interceptor);
            }}
        }});
    }}";
    }
    private static void InjectSimpleDependenciesWithInterceptionInGenericAttribute(IncrementalGeneratorInitializationContext context, string level)
    {
        var template = new StringBuilder(@"
                             namespace ServiceGenerator;
                             using Microsoft.Extensions.DependencyInjection;
                             using Castle.DynamicProxy;
                             internal static partial class SourceGeneratorInjectDependencies
                             {{
                                 private static partial IServiceCollection Inject" + level + @"InterceptionNonInheritable
            (this IServiceCollection services)
                                             {{ 
                                                {0}
                                                return services;
                                             }}
                             }}
            ");
        var provider = context.SyntaxProvider.ForAttributeWithMetadataName($"ServiceGenerator.{level}InterceptionAttribute`1",
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
                var interceptor = info.attribute.AttributeClass!.TypeArguments[0].OriginalDefinition.ToString();
                var injections = info.attribute.ConstructorArguments[0].Values.Select(b => b.Value!.ToString()).ToArray();

                foreach (var injection in injections)
                {
                    builder.Append(
                        $"services.{level}InterceptWithTarget(typeof({injection}),typeof({interceptor}),typeof({GetType(info.symbol, true)}));");
                    builder.AppendLine();
                }

                if (!injections.Any())
                {
                    builder.Append(
                        $"services.{level}InterceptWithoutTarget(typeof({GetType(info.symbol, true)}),typeof({interceptor}));");
                    builder.AppendLine();
                }
            }

            var injected = string.Format(template.ToString(), builder);
            a.AddSource($"{level}.Interceptor.NonInheritable.g.cs", injected);
        });
    }

    private static void InjectInheritableDependenciesWithInterceptionInGenericAttribute(IncrementalGeneratorInitializationContext context, string level)
    {
        var template = new StringBuilder(@"
                             namespace ServiceGenerator;
                             using Microsoft.Extensions.DependencyInjection;
                             using Castle.DynamicProxy;
                             internal static partial class SourceGeneratorInjectDependencies
                             {{
                                 private static partial IServiceCollection Inject" + level + @"InterceptionInheritable
            (this IServiceCollection services)
                                             {{ 
                                                {0}
                                                return services;
                                             }}
                                    " + GetInterceptionHelper(level) + @"
    }}
            ");
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
                var injections = InheritableInjections(semanticModel, classDeclaration, $"ServiceGenerator.Inheritable{level}InterceptionAttribute<>");

                return injections.SelectMany(injection => injection.types.Select(type =>
                    $"services.{level}InterceptWithTarget(typeof({type}),typeof({injection.generic}),typeof({GetType(classSymbol!, true)}));"));
            }).Collect();
        // Step 3: Check for the parent class attribute
        context.RegisterSourceOutput(classDeclarationsWithSemanticModel, (ctx, injections) =>
        {
            var injected = string.Format(template.ToString(), string.Join("\n", injections));
            ctx.AddSource($"{level}.Interceptor.Inheritable.g.cs", injected);
        });
    }

    private static void InjectInheritableWithInterfaces(IncrementalGeneratorInitializationContext context, string level)
    {
        var template = new StringBuilder(@"
                             namespace ServiceGenerator;
                             using Microsoft.Extensions.DependencyInjection;
                             using Castle.DynamicProxy;
                             internal static partial class SourceGeneratorInjectDependencies
                             {{
                                 private static partial IServiceCollection InjectInheritable" + level + @"WithInterfaces
            (this IServiceCollection services)
                                             {{ 
                                                {0}
                                                return services;
                                             }}
    }}
            ");
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
                var injections = InheritableInjections(semanticModel, classDeclaration, $"ServiceGenerator.Inheritable{level}InterfacesAttribute");

                return injections.SelectMany(_ => classSymbol.Interfaces.Select(type => $"services.Add{level}(typeof({GetType(type, true)}),typeof({GetType(classSymbol!, true)}));"));
            }).Collect();
        // Step 3: Check for the parent class attribute
        context.RegisterSourceOutput(classDeclarationsWithSemanticModel, (ctx, injections) =>
        {
            var injected = string.Format(template.ToString(), string.Join("\n", injections));
            ctx.AddSource($"{level}.AllInterfaces.Inheritable.g.cs", injected);
        });
    }


    private static void InitializeWithInterceptionInheritableInjectInterfaces(IncrementalGeneratorInitializationContext context, string level)
    {
        var template = new StringBuilder(@"
                             namespace ServiceGenerator;
                             using Microsoft.Extensions.DependencyInjection;
                             using Castle.DynamicProxy;
                             internal static partial class SourceGeneratorInjectDependencies
                             {{
                                 private static partial IServiceCollection InjectInheritable" + level + @"InterceptInterfaces
            (this IServiceCollection services)
                                             {{ 
                                                {0}
                                                return services;
                                             }}
    }}
            ");
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
                var injections = InheritableInjections(semanticModel, classDeclaration, $"ServiceGenerator.Inheritable{level}InterceptInterfacesAttribute<>");

                return injections.SelectMany(injection => classSymbol.Interfaces.Select(type => $"services.{level}InterceptWithTarget(typeof({GetType(type, true)}),typeof({injection.generic}),typeof({GetType(classSymbol!, true)}));"));
            }).Collect();
        // Step 3: Check for the parent class attribute
        context.RegisterSourceOutput(classDeclarationsWithSemanticModel, (ctx, injections) =>
        {
            var injected = string.Format(template.ToString(), string.Join("\n", injections));
            ctx.AddSource($"{level}.Interceptor.AllInterfaces.Inheritable.g.cs", injected);
        });
    }

    private static void InjectSimpleDependenciesForTypesInGenericAttribute(IncrementalGeneratorInitializationContext context, string level)
    {
        var template = new StringBuilder(@"
                             namespace ServiceGenerator;
                             using Microsoft.Extensions.DependencyInjection;
                             internal static partial class SourceGeneratorInjectDependencies
                             {{
                                 private static partial IServiceCollection Inject" + level + @"(this IServiceCollection services)
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
                var injections = MergeString(GetTypeOfParametricAttribute(info.attribute));
                foreach (var injection in injections)
                {

                    builder.Append(
                        $"services.Add{level}(typeof({injection}),typeof({GetType(info.symbol, true)}));");
                    builder.AppendLine();
                }
            }

            var injected = string.Format(template.ToString(), builder);
            a.AddSource($"{level}.g.cs", injected);
        });

    }
    private static string GetType(INamedTypeSymbol symbol, bool withSpecificationOfGeneric)
    {
        if (!symbol.IsGenericType)
        {
            return symbol.OriginalDefinition.ToString();
        }

        if (!symbol.TypeArguments.Any() || !withSpecificationOfGeneric)
        {
            return symbol.ConstructUnboundGenericType().ToString();
        }

        if (!symbol.TypeArguments.All(a => a.TypeKind is TypeKind.Class or TypeKind.Interface))
        {
            return symbol.ConstructUnboundGenericType().ToString();
        }

        return symbol.ToDisplayString();
    }

    private static void InjectSimpleDependenciesForTypesInAttribute(IncrementalGeneratorInitializationContext context, string level)
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

            var builder = new StringBuilder();

            foreach (var info in infos)
            {

                builder.AppendLine();
                var injections = MergeString(GetTypeOfParametricAttribute(info.attribute));
                foreach (var injection in injections)
                {

                    builder.Append(
                        $"services.Add{level}(typeof({injection}),typeof({GetType(info.symbol, true)}));");
                    builder.AppendLine();
                }
            }

            var injected = string.Format(template.ToString(), builder);
            a.AddSource($"{level}.Generic.g.cs", injected);
        });

    }
    private static (string generic, List<string> parameters) GetTypeOfParametricAttribute(AttributeData attribute)
    {
        var generic = "";
        List<string> parameters = new List<string>();
        if (attribute.AttributeClass!.IsGenericType)
        {
            generic = attribute.AttributeClass!.TypeArguments.Select(t => t.OriginalDefinition.ToString()).ToArray()[0];
        }
        if (attribute.ConstructorArguments.Any() && attribute.ConstructorArguments[0].Values is IEnumerable<TypedConstant> types)
        {
            parameters = types.Select(t => t.Value!.ToString()).ToList();
        }
        return (generic, parameters);
    }

    private static List<string> MergeString((string generic, List<string> parameters) tuple) => string.IsNullOrEmpty(tuple.generic)
        ? tuple.parameters
        : tuple.parameters.Union([tuple.generic]).ToList();
}