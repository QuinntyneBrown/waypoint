// Acceptance Test
// Traces to: L2-NFR-011 (liveness /health + readiness /health/ready with dependency probe)
using System.Net;

namespace Waypoint.IntegrationTests.NonFunctional;

[TestFixture]
[Property("Req", "L2-NFR-011")]
public sealed class HealthReadinessTests : IntegrationTestBase
{
    [Test]
    [Property("Req", "L2-NFR-011")]
    public async Task Liveness_probe_reports_process_up()
    {
        // Traces to: L2-NFR-011 AC1
        var response = await Client.GetAsync("/health");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    [Property("Req", "L2-NFR-011")]
    public async Task Readiness_probe_reports_database_reachable()
    {
        // Traces to: L2-NFR-011 AC1 — the fixture's real database is up, so readiness is healthy.
        var response = await Client.GetAsync("/health/ready");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}
