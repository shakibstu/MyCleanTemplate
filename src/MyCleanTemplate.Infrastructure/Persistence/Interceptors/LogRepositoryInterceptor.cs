using Castle.DynamicProxy;
using MyCleanTemplate.Common.Dependencies;
using MyCleanTemplate.Common.Logs;
using MyCleanTemplate.Domain.Abstractions;
using Newtonsoft.Json;
using Serilog;
using Serilog.Context;

namespace MyCleanTemplate.Infrastructure.Persistence.Interceptors
{
    internal class LogRepositoryInterceptor : ILogRepositoryInterceptor, IScopeDependency
    {
        private readonly ILogger _logger;
        public LogRepositoryInterceptor(ILogger logger)
        {
            _logger = logger;
        }
        public void Intercept(IInvocation invocation)
        {
            var name = invocation.Method.DeclaringType!.Namespace + "." + invocation.Method.DeclaringType!.Name + "." + invocation.Method.Name;
            var arguments = invocation.Arguments;
            try
            {
                invocation.Proceed();
                LogRepo(name, arguments, invocation.ReturnValue);
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
            using (LogContext.PushProperty("Arguments", arguments))
            using (LogContext.PushProperty("Response", JsonConvert.SerializeObject(valueResponse, Formatting.Indented)))
            using (LogContext.PushProperty(nameof(LogLevel), LogLevel.Repo))
            {
                _logger.Information("Repository -> {Method}", name);
            }
        }
    }
}
