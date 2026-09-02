// Acceptance Test
// Traces to: L2-NFR-002 (security headers present on every response)
using System.Net;

namespace Waypoint.IntegrationTests.NonFunctional;

[TestFixture]
[Property("Req", "L2-NFR-002")]
public sealed class SecurityHeadersTests : IntegrationTestBase
{
    [Test]
    [Property("Req", "L2-NFR-002")]
    public async Task Every_response_carries_baseline_security_headers()
    {
        // Traces to: L2-NFR-002
        var response = await Client.GetAsync("/health");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Headers.Contains("X-Content-Type-Options"), Is.True);
        Assert.That(response.Headers.GetValues("X-Content-Type-Options"), Does.Contain("nosniff"));
        Assert.That(response.Headers.Contains("X-Frame-Options"), Is.True);
        Assert.That(response.Headers.Contains("Referrer-Policy"), Is.True);
        Assert.That(response.Headers.Contains("Content-Security-Policy"), Is.True);
        Assert.That(response.Headers.Contains("Strict-Transport-Security"), Is.True);
    }

    [Test]
    [Property("Req", "L2-NFR-002")]
    public async Task Security_headers_also_ride_on_error_responses()
    {
        // Traces to: L2-NFR-002 — headers are set first in the pipeline, so they cover 4xx/5xx too.
        var response = await Client.GetAsync("/api/people"); // 401: no token

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(response.Headers.Contains("Content-Security-Policy"), Is.True);
        Assert.That(response.Headers.Contains("X-Content-Type-Options"), Is.True);
    }
}
