using Microsoft.CodeAnalysis;

namespace MyCleanTemplate.SourceGenerator
{
    [Generator]
    public class SingletonInjectIncrementalDependencies : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            InjectIncrementalDependencies.InitializeWithLevel(context, "Singleton");
        }
    }
}
