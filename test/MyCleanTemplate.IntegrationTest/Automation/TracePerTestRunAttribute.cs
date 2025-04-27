using System.Diagnostics;
using System.Reflection;
using MyCleanTemplate.Domain.Common.Logs;
using Xunit.v3;

namespace MyCleanTemplate.IntegrationTest.Automation;

public class TracePerTestRunAttribute : BeforeAfterTestAttribute
{
    private Activity? activityForThisTest;
    internal static Activity? ActivityForTestRun = TelemetryProperties.MainActivitySource.StartActivity("TestRun");

    public override void Before(MethodInfo methodUnderTest, IXunitTest test)
    {
        if (ActivityForTestRun == null)
            throw new ArgumentNullException(nameof(ActivityForTestRun),
                "The test run Activity was null, and therefore can't be used");

        activityForThisTest = TelemetryProperties.MainActivitySource
            .StartActivity(methodUnderTest.Name,
                ActivityKind.Internal,
                ActivityForTestRun.Context);
        base.Before(methodUnderTest, test);
    }

    public override void After(MethodInfo methodUnderTest, IXunitTest test)
    {
        activityForThisTest?.Stop();
        base.After(methodUnderTest, test);
    }

}