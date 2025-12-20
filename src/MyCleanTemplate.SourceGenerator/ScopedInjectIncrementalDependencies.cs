using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace MyCleanTemplate.SourceGenerator;

[Generator]
public class ScopedInjectIncrementalDependencies : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
//#if DEBUG
//        Debugger.Launch();
//#endif
        InjectIncrementalDependencies.InitializeWithLevel(context, "Scoped");
    }
}