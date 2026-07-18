// Acceptance Test
// Traces to: L2-PLATFORM-006
using System.Net;

namespace WordUp.IntegrationTests.Platform;

[TestFixture]
[Property("Req", "L2-PLATFORM-006")]
public sealed class ForbiddenStateTests : IntegrationTestBase
{
    [Test]
    [Property("Req", "L2-PLATFORM-006")]
    public async Task A_youth_calling_an_admin_only_endpoint_is_forbidden_and_no_people_data_is_leaked()
    {
        // Traces to: L2-PLATFORM-006
        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/people");
        Authorize(request, youthToken);
        var response = await Client.SendAsync(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        // A denied request must not leak any of the protected people/email data.
        var body = await response.Content.ReadAsStringAsync();
        Assert.That(body, Does.Not.Contain("@"));
    }
}
