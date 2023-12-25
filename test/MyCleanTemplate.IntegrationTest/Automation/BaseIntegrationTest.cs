using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MyCleanTemplate.Infrastructure.Persistence;
using Xunit;

namespace MyCleanTemplate.IntegrationTest.Automation
{
    public abstract class BaseIntegrationTest(IntegrationTestWebApplicationFactory applicationFactory)
        : IClassFixture<IntegrationTestWebApplicationFactory>, IAsyncLifetime
    {
        protected ISender Sender = null!;
        protected HttpClient HttpClient = null!;
        protected IServiceProvider ServiceProvider => _scope!.ServiceProvider;
        private IServiceScope? _scope;

        public Task InitializeAsync()
        {
            _scope = applicationFactory.Services.CreateScope();
            Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
            HttpClient = applicationFactory.CreateClient();
            return ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreatedAsync();
        }

        public Task DisposeAsync()
        {
            _scope?.Dispose();
            HttpClient.Dispose();
            return Task.CompletedTask;
        }
    }
}
