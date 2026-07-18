using System.Net;
using System.Net.Http.Json;
using WordUp.Application.Authentication.Auth;

namespace WordUp.IntegrationTests.Auth;

// Acceptance Test
// Traces to: L2-AUTH-012 (token security & session management)
[TestFixture]
[Property("Req", "L2-AUTH-012")]
public sealed class RefreshTokenTests : IntegrationTestBase
{
    private const string Password = "Passw0rd!";

    private async Task<AuthResult> RegisterAsync(string email)
    {
        var res = await Client.PostAsJsonAsync("/api/auth/register",
            new { email, password = Password, displayName = "Refresh Test" });
        res.EnsureSuccessStatusCode();
        return (await res.Content.ReadFromJsonAsync<AuthResult>())!;
    }

    [Test]
    [Property("Req", "L2-AUTH-012")]
    public async Task Refreshing_rotates_the_refresh_token()
    {
        var session = await RegisterAsync($"rotate.{Guid.NewGuid():N}@example.com");

        var response = await Client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = session.RefreshToken });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var rotated = await response.Content.ReadFromJsonAsync<AuthResult>();
        Assert.That(rotated!.RefreshToken, Is.Not.Empty);
        Assert.That(rotated.RefreshToken, Is.Not.EqualTo(session.RefreshToken), "the refresh token must rotate");
    }

    [Test]
    [Property("Req", "L2-AUTH-012")]
    public async Task Reusing_a_rotated_refresh_token_is_rejected_and_revokes_the_whole_family()
    {
        var session = await RegisterAsync($"reuse.{Guid.NewGuid():N}@example.com");

        var firstRotation = await (await Client.PostAsJsonAsync("/api/auth/refresh",
            new { refreshToken = session.RefreshToken })).Content.ReadFromJsonAsync<AuthResult>();

        // Present the original (already-rotated) token again -> reuse detected.
        var reuse = await Client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = session.RefreshToken });
        Assert.That(reuse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));

        // Reuse detection kills the family, so the freshly-issued token is dead too.
        var afterFamilyRevoke = await Client.PostAsJsonAsync("/api/auth/refresh",
            new { refreshToken = firstRotation!.RefreshToken });
        Assert.That(afterFamilyRevoke.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
