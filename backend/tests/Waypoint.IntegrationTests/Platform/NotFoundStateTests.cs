// Acceptance Test
// Traces to: L2-PLATFORM-007
using System.Net;

namespace Waypoint.IntegrationTests.Platform;

[TestFixture]
[Property("Req", "L2-PLATFORM-007")]
public sealed class NotFoundStateTests : IntegrationTestBase
{
    [Test]
    [Property("Req", "L2-PLATFORM-007")]
    public async Task A_well_formed_but_unmapped_route_returns_not_found()
    {
        // Traces to: L2-PLATFORM-007
        var response = await Client.GetAsync("/api/this-does-not-exist");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
