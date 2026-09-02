// Acceptance Tests
// Traces to: L2-NFR-011, L2-PLATFORM-008
using System.Net;
using System.Net.Http.Headers;

namespace Waypoint.IntegrationTests.Platform;

[TestFixture]
[Property("Req", "L2-NFR-011")]
public sealed class ObservabilityTests : IntegrationTestBase
{
    [Test]
    [Property("Req", "L2-NFR-011")]
    public async Task Request_logs_are_structured_correlated_and_exclude_query_and_authorization_values()
    {
        // Traces to: L2-NFR-011 AC2
        const string correlationId = "structured-request-correlation";
        const string email = "private.person@example.test";
        const string bearerSecret = "sensitive-bearer-token-value";
        const string routeSecret = "guardian-consent-secret-token";
        Factory.Logs.Clear();

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/health?email={Uri.EscapeDataString(email)}&access_token={bearerSecret}");
        request.Headers.Add("X-Correlation-ID", correlationId);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerSecret);
        var response = await Client.SendAsync(request);
        var tokenRouteResponse = await Client.GetAsync($"/api/public/guardian-consent/{routeSecret}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(tokenRouteResponse.StatusCode, Is.EqualTo(HttpStatusCode.Gone));
        var entry = Factory.Logs.Entries.Single(log =>
            log.Category == "Waypoint.Api.Middleware.RequestLoggingMiddleware"
            && log.Properties.GetValueOrDefault("CorrelationId")?.ToString() == correlationId);
        Assert.Multiple(() =>
        {
            Assert.That(entry.Properties.GetValueOrDefault("Method"), Is.EqualTo("GET"));
            Assert.That(entry.Properties.GetValueOrDefault("Route"), Is.EqualTo("/health"));
            Assert.That(entry.Properties.GetValueOrDefault("StatusCode"), Is.EqualTo(200));
            Assert.That(entry.Properties.ContainsKey("ElapsedMilliseconds"), Is.True);
        });
        var tokenRouteEntry = Factory.Logs.Entries.Single(log =>
            log.Category == "Waypoint.Api.Middleware.RequestLoggingMiddleware"
            && log.Properties.GetValueOrDefault("Route")?.ToString()?.Contains("guardian-consent") == true);
        Assert.That(tokenRouteEntry.Properties.GetValueOrDefault("Route")?.ToString(),
            Does.Contain("{token}"));

        var telemetry = Render(Factory.Logs.Entries);
        Assert.That(telemetry, Does.Not.Contain(email));
        Assert.That(telemetry, Does.Not.Contain(bearerSecret));
        Assert.That(telemetry, Does.Not.Contain(routeSecret));
    }

    [Test]
    [Property("Req", "L2-PLATFORM-008")]
    [Property("Req", "L2-NFR-011")]
    public async Task Unhandled_errors_log_and_alert_only_redacted_operational_context()
    {
        // Traces to: L2-PLATFORM-008 AC1, L2-NFR-011 AC2
        const string correlationId = "redacted-error-correlation";
        const string sensitiveEmail = "diagnostic.person@example.test";
        const string sensitiveSecret = "Bearer diagnostic-secret-token";
        Factory.Logs.Clear();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/_diag/throw");
        request.Headers.Add("X-Correlation-ID", correlationId);
        var response = await Client.SendAsync(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        var errorLog = Factory.Logs.Entries.Single(log =>
            log.Category == "Waypoint.Api.UnhandledException"
            && log.Properties.GetValueOrDefault("CorrelationId")?.ToString() == correlationId);
        var alert = Factory.ExceptionAlerts.Alerts.Single();
        var telemetry = Render(Factory.Logs.Entries) + Environment.NewLine
            + string.Join(Environment.NewLine, Factory.ExceptionAlerts.Alerts);
        Assert.Multiple(() =>
        {
            Assert.That(errorLog.Exception, Is.Null);
            Assert.That(errorLog.Properties.GetValueOrDefault("ExceptionType"),
                Is.EqualTo(nameof(InvalidOperationException)));
            Assert.That(errorLog.Properties.GetValueOrDefault("Route")?.ToString(),
                Does.Contain("_diag/throw"));
            Assert.That(alert.ExceptionType, Is.EqualTo(nameof(InvalidOperationException)));
            Assert.That(alert.CorrelationId, Is.EqualTo(correlationId));
            Assert.That(alert.Route, Does.Contain("_diag/throw"));
            Assert.That(telemetry, Does.Contain(correlationId));
            Assert.That(telemetry, Does.Contain(nameof(InvalidOperationException)));
            Assert.That(telemetry, Does.Not.Contain(sensitiveEmail));
            Assert.That(telemetry, Does.Not.Contain(sensitiveSecret));
        });
    }

    private static string Render(IEnumerable<CapturedLogEntry> entries) => string.Join(
        Environment.NewLine,
        entries.Select(entry =>
            $"{entry.Category} {entry.Level} {entry.Message} {entry.Exception} "
            + string.Join(" ", entry.Properties.Select(property => $"{property.Key}={property.Value}"))));
}
