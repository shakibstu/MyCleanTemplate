using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace MyCleanTemplate.Domain.Common.Logs;

public static class TelemetryHelper
{
    public static readonly ActivitySource MainActivitySource = new(TelemetryServiceName);
    public const string TelemetryServiceName = "Telemetry.MyCleanTemplate";
    public const string TelemetrySpanLevelApi = "Telemetry.SpanLevel.API";
    public const string TelemetrySpanLevelMediator = "Telemetry.SpanLevel.Mediator";
    public const string TelemetrySpanLevelRepository = "Telemetry.SpanLevel.Repository";

    public static Meter Meter = new Meter(TelemetryServiceName);
    public static Histogram<double> TestValue = Meter.CreateHistogram<double>("test.value");

}