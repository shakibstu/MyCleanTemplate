using System.Diagnostics;

namespace MyCleanTemplate.Domain.Common.Logs;

public static class TelemetryProperties
{
    public static readonly ActivitySource MainActivitySource = new(LogKeywords.TelemetryServiceName);
}