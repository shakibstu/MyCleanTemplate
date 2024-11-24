using Microsoft.CodeAnalysis;

namespace MyCleanTemplate.SourceGenerator
{
    [Generator]
    public class ScopedInjectIncrementalDependencies : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            InjectIncrementalDependencies.InitializeWithLevel(context, "Scoped");
        }
    }
}
