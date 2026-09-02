using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Waypoint.IntegrationTests.Platform;

// Acceptance Test
// Traces to: L2-PLATFORM-009 (maintenance mode: API returns 503 with Retry-After; health probe stays up)
[TestFixture]
[Property("Req", "L2-PLATFORM-009")]
public sealed class MaintenanceModeTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        // Flip maintenance mode on for a dedicated host; the flag short-circuits every request but /health.
        _factory = new SqlServerWebAppFactory().WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Maintenance:Enabled"] = "true",
                })));
        _client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    [Property("Req", "L2-PLATFORM-009")]
    public async Task Api_calls_return_503_with_retry_after_during_maintenance()
    {
        // Traces to: L2-PLATFORM-009
        var response = await _client.PostAsJsonAsync("/api/auth/sign-in", new { email = "x@example.com", password = "whatever" });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        Assert.That(response.Headers.Contains("Retry-After"), Is.True);
    }

    [Test]
    [Property("Req", "L2-PLATFORM-009")]
    public async Task Health_probe_stays_available_during_maintenance()
    {
        // Traces to: L2-PLATFORM-009
        var response = await _client.GetAsync("/health");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}
