using System.Net;

namespace Waypoint.IntegrationTests;

// Walking-skeleton smoke: the API boots with the full middleware pipeline and serves ops endpoints.
[TestFixture]
public sealed class SmokeTests : IntegrationTestBase
{
    [Test]
    public async Task Health_endpoint_reports_healthy()
    {
        var response = await Client.GetAsync("/health");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("Healthy"));
    }

    [Test]
    public async Task Swagger_document_is_served()
    {
        var response = await Client.GetAsync("/swagger/v1/swagger.json");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}
