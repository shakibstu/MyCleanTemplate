using Microsoft.CodeAnalysis;
using System.Diagnostics;

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