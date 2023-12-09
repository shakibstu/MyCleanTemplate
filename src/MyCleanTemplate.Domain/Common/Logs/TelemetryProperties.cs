using System.Collections.Concurrent;
using System.Diagnostics;
using OpenTelemetry.Trace;

namespace MyCleanTemplate.Domain.Common.Logs
{
    public static class TelemetryProperties
    {
        public static readonly ActivitySource MainActivitySource = new(LogKeywords.TelemetryServiceName);
    }
}
