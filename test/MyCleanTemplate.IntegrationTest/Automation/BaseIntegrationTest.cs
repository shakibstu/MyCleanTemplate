using Microsoft.Extensions.DependencyInjection;
using MyCleanTemplate.Domain.Abstractions.Messaging;
using MyCleanTemplate.Infrastructure.Persistence;
using Xunit;

namespace MyCleanTemplate.IntegrationTest.Automation;

public abstract class BaseIntegrationTest(IntegrationTestWebApplicationFactory applicationFactory)
    : IClassFixture<IntegrationTestWebApplicationFactory>, IAsyncLifetime
{
    protected IMediator Sender = null!;
    protected HttpClient HttpClient = null!;
    protected IServiceProvider ServiceProvider => _scope!.ServiceProvider;
    private IServiceScope? _scope;

    public async ValueTask InitializeAsync()
    {
        _scope = applicationFactory.Services.CreateScope();
        Sender = _scope.ServiceProvider.GetRequiredService<IMediator>();
        HttpClient = applicationFactory.CreateClient();
        await ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreatedAsync();
    }

    public ValueTask DisposeAsync()
    {
        _scope?.Dispose();
        HttpClient.Dispose();
        return ValueTask.CompletedTask;
    }
}