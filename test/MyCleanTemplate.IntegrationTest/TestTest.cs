using MyCleanTemplate.IntegrationTest.Automation;
using System.Net.Http.Json;
using Shouldly;
using Xunit;

namespace MyCleanTemplate.IntegrationTest
{

    public class TestTest : BaseIntegrationTest
    {
        [Fact]
        public async Task TestGet()
        {
            var response = await HttpClient.PostAsJsonAsync("/api/test/GetTest", new object());
            var x = await response.Content.ReadAsStringAsync();
            x.ShouldNotBeEmpty();
        }

        public TestTest(IntegrationTestWebApplicationFactory applicationFactory) : base(applicationFactory)
        {
        }
    }
}