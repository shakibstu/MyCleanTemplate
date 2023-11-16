using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyCleanTemplate.IntegrationTest.Automation;
using System.Net.Http.Json;

namespace MyCleanTemplate.IntegrationTest
{
    [TestClass]
    public class TestTest : BaseIntegrationTest
    {
        [TestMethod]
        public async Task TestGet()
        {
            var response = await HttpClient.PostAsJsonAsync("/api/test/GetTest", new object());
            var x = await response.Content.ReadAsStringAsync();
        }
    }
}