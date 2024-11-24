using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace MyCleanTemplate.SourceGenerator
{
    [Generator]
    public class TransientInjectIncrementalDependencies : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif
            InjectIncrementalDependencies.InitializeWithLevel(context, "Transient");
        }
    }
}
