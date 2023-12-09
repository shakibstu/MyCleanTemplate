namespace MyCleanTemplate.Domain.Common.Logs
{
    public static class LogKeywords
    {
        public static string ApiRId => "API.RId";
        public static string AppRId => "App.RId";
        public static string SourceContext => "MainLogger";

        public static string TelemetryServiceName => "Telemetry.MyCleanTemplate";
        public static string TelemetrySpanLevelApi => "Telemetry.SpanLevel.API";
        public static string TelemetrySpanLevelApp => "Telemetry.SpanLevel.App";
        public static string TelemetrySpanLevelDb => "Telemetry.SpanLevel.Db";

    }
}
