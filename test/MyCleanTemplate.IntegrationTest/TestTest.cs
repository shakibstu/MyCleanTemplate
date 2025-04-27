using MyCleanTemplate.IntegrationTest.Automation;
using System.Net.Http.Json;
using MyCleanTemplate.Application.Contract.Test.Query;
using Shouldly;
using Xunit;
using MyCleanTemplate.API.Models;

namespace MyCleanTemplate.IntegrationTest;

[TracePerTestRun]
public class TestTest(IntegrationTestWebApplicationFactory applicationFactory)
    : BaseIntegrationTest(applicationFactory)
{
    [Fact]
    public async Task TestGet()
    {
        var response = await HttpClient.PostAsJsonAsync("/api/test/GetTest", new object());
        var rootSpan = OtelTestFramework.CollectedSpans.GetSpanByName("start-get", OtelTestFramework.RunId)
            .FirstOrDefault();
        var x = await response.Content.ReadAsStringAsync();
        x.ShouldNotBeEmpty();
    }
        
    [Fact]
    public async Task TestGetApp()
    {
        var response = await Sender.Send(new TestQuery());
        response.ShouldNotBeNull();
    }
    [Fact]
    public async Task TestCreate()
    {
        var request = new CreateTestRequest(description: Guid.NewGuid().ToString(), id: Guid.NewGuid(),
            name: Guid.NewGuid().ToString(), startTime: DateTime.UtcNow);
        var response = await HttpClient.PostAsJsonAsync("/api/test/CreateTest", request);
        var rootSpan = OtelTestFramework.CollectedSpans.GetSpanByName("start-get", OtelTestFramework.RunId)
            .FirstOrDefault();
        var x = await response.Content.ReadAsStringAsync();
        x.ShouldNotBeEmpty();
    }
}