using MyCleanTemplate.IntegrationTest.Automation;
using System.Net.Http.Json;
using MyCleanTemplate.Application.Contract.Test.Query;
using Shouldly;
using Xunit;

namespace MyCleanTemplate.IntegrationTest;

public class TestTest(IntegrationTestWebApplicationFactory applicationFactory)
    : BaseIntegrationTest(applicationFactory)
{
    [Fact]
    public async Task TestGet()
    {
        var response = await HttpClient.PostAsJsonAsync("/api/test/GetTest", new object());
        var x = await response.Content.ReadAsStringAsync();
        x.ShouldNotBeEmpty();
    }
        
    [Fact]
    public async Task TestGetApp()
    {
        var response = await Sender.Send(new TestQuery());
        response.ShouldNotBeNull();
    }
}