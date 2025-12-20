using OpenTelemetry;
using System.Diagnostics;

namespace MyCleanTemplate.IntegrationTest.Automation;

public class TestRunSpanProcessor : BaseProcessor<Activity>
{
    private readonly string _testRunId;

    public TestRunSpanProcessor(string testRunId)
    {
        _testRunId = testRunId;
    }

    public override void OnStart(Activity data)
    {
        data?.SetTag("test.run_id", _testRunId);
    }
}