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
        var response = await Client.GetAsync("/api/_diag/throw");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));

        var body = await response.Content.ReadAsStringAsync();
        // A friendly, generic title is surfaced...
        Assert.That(body, Does.Contain("An unexpected error occurred."));
        // ...but no exception type or stack trace leaks to the client.
        Assert.That(body, Does.Not.Contain("InvalidOperationException"));
        Assert.That(body, Does.Not.Contain("   at WordUp"));
    }
}
