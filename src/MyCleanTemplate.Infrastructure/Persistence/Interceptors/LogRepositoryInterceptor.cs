using Castle.DynamicProxy;
using MyCleanTemplate.Domain.Common.Logs;
using Serilog.Context;
using ServiceGenerator;
using System.Text.Json;
using ILogger = Serilog.ILogger;
namespace MyCleanTemplate.Infrastructure.Persistence.Interceptors
{
    internal class LogRepositoryInterceptor(ILogger logger) : ILogRepositoryInterceptor, IScopeDependency
    {
        public void Intercept(IInvocation invocation)
        {
            var name = invocation.Method.DeclaringType!.Namespace + "." + invocation.Method.DeclaringType!.Name + "." + invocation.Method.Name;
            var arguments = invocation.Arguments;
            try
            {
                using (LogContext.PushProperty("Repository.RId", Guid.NewGuid()))
                {
                    invocation.Proceed();
                    LogRepo(name, arguments, invocation.ReturnValue);
                }
            }
            catch (Exception ex)
            {
                LogRepo(name, arguments, ex);
                throw;
            }

        }

        private void LogRepo(string name, object[] arguments, object response)
        {
            var valueResponse = response.GetType().GetProperties().First(a => a.Name == "Result").GetValue(response, null);

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
