// Acceptance Test
// Traces to: L2-PLATFORM-008
using System.Net;

namespace WordUp.IntegrationTests.Platform;

[TestFixture]
[Property("Req", "L2-PLATFORM-008")]
public sealed class ServerErrorStateTests : IntegrationTestBase
{
    [Test]
    [Property("Req", "L2-PLATFORM-008")]
    public async Task An_unhandled_error_returns_a_generic_500_without_leaking_internal_detail()
    {
        // Traces to: L2-PLATFORM-008
        const string correlationId = "server-error-acceptance-correlation";
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/_diag/throw");
        request.Headers.Add("X-Correlation-ID", correlationId);
        var response = await Client.SendAsync(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));

        var body = await response.Content.ReadAsStringAsync();
        // A friendly, generic title is surfaced...
        Assert.That(body, Does.Contain("An unexpected error occurred."));
        // ...but no exception type or stack trace leaks to the client.
        Assert.That(body, Does.Not.Contain("InvalidOperationException"));
        Assert.That(body, Does.Not.Contain("   at WordUp"));
        Assert.That(response.Headers.GetValues("X-Correlation-ID").Single(), Is.EqualTo(correlationId));
        Assert.That(body, Does.Contain(correlationId));
        var alert = Factory.ExceptionAlerts.Alerts.Single();
        Assert.That(alert.CorrelationId, Is.EqualTo(correlationId));
        Assert.That(alert.ExceptionType, Is.EqualTo(nameof(InvalidOperationException)));
    }
}
