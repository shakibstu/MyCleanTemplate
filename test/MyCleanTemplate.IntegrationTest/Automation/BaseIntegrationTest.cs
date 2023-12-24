using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MyCleanTemplate.IntegrationTest.Automation
{
    public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebApplicationFactory>
    {
        protected readonly ISender Sender;
        protected readonly HttpClient HttpClient;
        protected readonly IntegrationTestWebApplicationFactory ApplicationFactory;
        protected BaseIntegrationTest(IntegrationTestWebApplicationFactory applicationFactory)
        {
            var scope = applicationFactory.Services.CreateScope();
            Sender = scope.ServiceProvider.GetRequiredService<ISender>();
            HttpClient = applicationFactory.Server.CreateClient();
            ApplicationFactory = applicationFactory;
        }
    }
}
