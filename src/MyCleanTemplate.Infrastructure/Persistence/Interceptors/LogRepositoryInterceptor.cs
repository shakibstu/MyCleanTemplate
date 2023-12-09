using Castle.DynamicProxy;
using MyCleanTemplate.Domain.Common.Logs;
using Serilog.Context;
using ServiceGenerator;
using System.Text.Json;
using OpenTelemetry.Trace;
using ILogger = Serilog.ILogger;
namespace MyCleanTemplate.Infrastructure.Persistence.Interceptors
{
    internal class LogRepositoryInterceptor(ILogger logger, TracerProvider tracerProvider) : ILogRepositoryInterceptor, IScopeDependency
    {
        public void Intercept(IInvocation invocation)
        {
            var name = invocation.Method.DeclaringType!.Namespace + "." + invocation.Method.DeclaringType!.Name + "." + invocation.Method.Name;
            var arguments = invocation.Arguments;

            using var span = tracerProvider.GetTracer(LogKeywords.TelemetrySpanLevelDb)
                .StartActiveSpan(LogKeywords.TelemetryServiceName);
            using var activity =
                TelemetryProperties.MainActivitySource.StartActivity(LogKeywords.TelemetrySpanLevelDb);
            activity?.SetTag("Repository.Arguments", arguments);
            activity?.SetTag("Repository.MethodName", invocation);
            try
            {
                using (LogContext.PushProperty("Repository.RId", Guid.NewGuid()))
                {
                    invocation.Proceed();

                    var valueResponse = invocation.ReturnValue.GetType().GetProperties().First(a => a.Name == "Result")
                        .GetValue(invocation.ReturnValue, null);

                    activity?.SetTag("Repository.Response", JsonSerializer.Serialize(valueResponse));

                    LogRepo(name, arguments, invocation.ReturnValue);
                }
            }
            catch (Exception ex)
            {
                LogRepo(name, arguments, ex);
                throw;
            }

        }

        private void LogRepo(string name, object[] arguments, object valueResponse)
        {
            
            using (LogContext.PushProperty(nameof(LogKeywords.SourceContext), LogKeywords.SourceContext))
            using (LogContext.PushProperty("Repository.Arguments", arguments))
            using (LogContext.PushProperty("Repository.Response", JsonSerializer.Serialize(valueResponse)))
            using (LogContext.PushProperty("Repository.MethodName", name))
            using (LogContext.PushProperty(nameof(LogLevel), LogLevel.Repo))
            {
                logger.Information("Repository Response");
            }
        }
    }
}
