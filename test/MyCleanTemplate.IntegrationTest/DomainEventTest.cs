using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using MyCleanTemplate.API.Models;
using MyCleanTemplate.Domain.Events;
using MyCleanTemplate.IntegrationTest.Automation;
using Shouldly;
using Xunit;

namespace MyCleanTemplate.IntegrationTest;

public class DomainEventTest(IntegrationTestWebApplicationFactory applicationFactory)
    : BaseIntegrationTest(applicationFactory)
{
    [Fact]
    public async Task CreateTest_DispatchesTestAddedEvent_ToNotificationHandlers()
    {
        var name = $"event-{Guid.NewGuid():N}";
        var probe = ServiceProvider.GetRequiredService<NotificationProbe>();

        var response = await HttpClient.PostAsJsonAsync("/api/test/CreateTest",
            new CreateTestRequest { Name = name }, TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var received = await probe.WaitForAsync<TestAddedEvent>(e => e.AddedTestEntity.Name == name,
            TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);

        received.ShouldNotBeNull("TestAddedEvent never reached a notification handler");
    }
}
