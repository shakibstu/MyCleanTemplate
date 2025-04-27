using MyCleanTemplate.Domain.Common.Logs;
using MyCleanTemplate.IntegrationTest.Automation;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PracticalOtel.xUnit.v3.OpenTelemetry;
using Xunit.v3;

[assembly: TestPipelineStartup(typeof(OtelTestFramework))]
namespace MyCleanTemplate.IntegrationTest.Automation
{
    public class OtelTestFramework : TracedPipelineStartup
    {
        public static readonly InMemoryTestSpans CollectedSpans = [];
        public static readonly Guid RunId = Guid.NewGuid();
        public OtelTestFramework() 
        {
            traceProviderSetup = tpb =>
            {
                tpb
                    .ConfigureResource(resource => resource.AddService(LogKeywords.TelemetryServiceName))
                    .AddSource("UnitTests")
                    .AddSource(TelemetryProperties.MainActivitySource.Name)
                    .AddInMemoryExporter(CollectedSpans)
                    .AddAspNetCoreInstrumentation()
                    .AddProcessor(new TestRunSpanProcessor(RunId.ToString()))
                    .AddOtlpExporter();
            };
        }
    }
}
