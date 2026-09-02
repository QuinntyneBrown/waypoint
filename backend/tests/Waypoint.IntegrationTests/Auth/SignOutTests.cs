using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Waypoint.Application.Authentication.Auth;

namespace Waypoint.IntegrationTests.Auth;

// Acceptance Test
// Traces to: L2-AUTH-007 (shared-device sign-out tears down the session)
[TestFixture]
[Property("Req", "L2-AUTH-007")]
public sealed class SignOutTests : IntegrationTestBase
{
    private const string Password = "Passw0rd!";

    [Test]
    [Property("Req", "L2-AUTH-007")]
    public async Task Signing_out_revokes_the_sessions_refresh_tokens()
    {
        var email = $"signout.{Guid.NewGuid():N}@example.com";
        var session = await (await Client.PostAsJsonAsync("/api/auth/register",
            new { email, password = Password, displayName = "SignOut Test" }))
            .Content.ReadFromJsonAsync<AuthResult>();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/sign-out");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session!.AccessToken);
        var signOut = await Client.SendAsync(request);
        Assert.That(signOut.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // The refresh token is now dead: no silent session restoration on a shared device.
        var refresh = await Client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = session.RefreshToken });
        Assert.That(refresh.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
