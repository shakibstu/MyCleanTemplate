using Castle.DynamicProxy;
using MyCleanTemplate.Domain.Common.Logs;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace MyCleanTemplate.Domain.Common.Messaging
{
    public class MediatorLogger : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            var arguments = invocation.Arguments;

            using var activity =
                TelemetryHelper.MainActivitySource.StartActivity(TelemetryHelper.TelemetrySpanLevelApp);
            try
            {
                activity?.SetTag("Mediator.Argument", arguments[0]);
                activity?.SetTag("Mediator.MethodName",
                    $"{invocation.TargetType?.FullName ?? ""}.{invocation.Method.Name}");
                invocation.Proceed();

                var valueResponse = invocation.ReturnValue?.GetType().GetProperties().First(a => a.Name == "Result")
                    .GetValue(invocation.ReturnValue, null);

                activity?.SetTag("Mediator.Response", JsonSerializer.Serialize(valueResponse));
            }
            catch (TargetInvocationException tex)
            {
                var realException = tex.InnerException?.InnerException ?? tex;
                activity?.AddException(realException);
                activity?.SetStatus(ActivityStatusCode.Error, realException.Message);
                throw realException;
            }
            catch (Exception e)
            {
                activity?.AddException(e);
                activity?.SetStatus(ActivityStatusCode.Error, e.Message);
                throw;
            }
        }
    }
}
