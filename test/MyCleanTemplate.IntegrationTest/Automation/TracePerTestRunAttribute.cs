using MyCleanTemplate.Domain.Common.Logs;
using System.Diagnostics;
using System.Reflection;
using Xunit.v3;

namespace MyCleanTemplate.IntegrationTest.Automation;

public class TracePerTestRunAttribute : BeforeAfterTestAttribute
{
    private Activity? _activityForThisTest;
    internal static Activity? ActivityForTestRun = TelemetryHelper.MainActivitySource.StartActivity("TestRun");

    public override void Before(MethodInfo methodUnderTest, IXunitTest test)
    {
        if (ActivityForTestRun == null)
            throw new ArgumentNullException(nameof(ActivityForTestRun),
                "The test run Activity was null, and therefore can't be used");

        _activityForThisTest = TelemetryHelper.MainActivitySource
            .StartActivity(methodUnderTest.Name,
                ActivityKind.Internal,
                ActivityForTestRun.Context);
        base.Before(methodUnderTest, test);
    }

    public override void After(MethodInfo methodUnderTest, IXunitTest test)
    {
        _activityForThisTest?.Stop();
        base.After(methodUnderTest, test);
    }

}