using MyCleanTemplate.API.Models;
using MyCleanTemplate.Application.Contract.Test.Query;
using MyCleanTemplate.Domain.Common.Logs;
using MyCleanTemplate.IntegrationTest.Automation;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace MyCleanTemplate.IntegrationTest;

[TracePerTestRun]
public class TestTest(IntegrationTestWebApplicationFactory applicationFactory)
    : BaseIntegrationTest(applicationFactory)
{
    [Fact]
    public async Task CreateTest()
    {
        var request = new CreateTestRequest
        {
            Name = "SH"
        };
        var response = await HttpClient.PostAsJsonAsync("/api/test/CreateTest", request);

        var activities = OtelTestFramework.CollectedSpans.GetSpans([CheckName!, CheckArgument!]
                , TelemetryHelper.TelemetrySpanLevelRepository, OtelTestFramework.RunId);
        activities.Count.ShouldBe(1);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return;

        bool CheckName(Dictionary<string, object> tags) => (string)tags["Repository.MethodName"] == "MyCleanTemplate.Infrastructure.Tests.TestRepository.CreateTest";

        bool CheckArgument(Dictionary<string, object> tags) => ((object[])tags["Repository.Arguments"])[0].ToString() == request.Name;
    }

    [Fact]
    public async Task TestGetApp()
    {
        var response = await Sender.Send(new TestQuery());
        response.ShouldNotBeNull();
    }
    //[Fact]
    //public async Task TestCreate()
    //{
    //    var request = new CreateTestRequest(description: Guid.NewGuid().ToString(), id: Guid.NewGuid(),
    //        name: Guid.NewGuid().ToString(), startTime: DateTime.UtcNow);
    //    var response = await HttpClient.PostAsJsonAsync("/api/test/CreateTest", request);
    //    var rootSpan = OtelTestFramework.CollectedSpans.GetSpanByName("start-get", OtelTestFramework.RunId)
    //        .FirstOrDefault();
    //    var x = await response.Content.ReadAsStringAsync();
    //    x.ShouldNotBeEmpty();
    //}
}