using Castle.DynamicProxy;
using MyCleanTemplate.Domain.Common.Logs;
using System.Text.Json;
namespace MyCleanTemplate.Infrastructure.Persistence.Interceptors;

internal class LogRepositoryInterceptor : IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        var arguments = invocation.Arguments;

        using var activity =
            TelemetryProperties.MainActivitySource.StartActivity(LogKeywords.TelemetrySpanLevelDb);
        activity?.SetTag("Repository.Arguments", arguments);
        activity?.SetTag("Repository.MethodName", invocation);
        invocation.Proceed();

        var valueResponse = invocation.ReturnValue.GetType().GetProperties().First(a => a.Name == "Result")
            .GetValue(invocation.ReturnValue, null);

        activity?.SetTag("Repository.Response", JsonSerializer.Serialize(valueResponse));

    }
}