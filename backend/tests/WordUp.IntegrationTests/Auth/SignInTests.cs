using System.Net;
using System.Net.Http.Json;
using WordUp.Application.Auth;

namespace WordUp.IntegrationTests.Auth;

// Acceptance Test
// Traces to: L2-AUTH-001 (email/password sign-in)
// Description: signing in with valid credentials issues an access token and returns the user's
//   identity; an unrecognized email and a wrong password fail identically so the response never
//   reveals whether an account exists (AC1, AC2, AC3).
[TestFixture]
[Property("Req", "L2-AUTH-001")]
public sealed class SignInTests : IntegrationTestBase
{
    private const string Password = "Passw0rd!";

    [Test]
    [Property("Req", "L2-AUTH-001")]
    public async Task Valid_credentials_issue_an_access_token_and_return_the_users_identity()
    {
        var email = $"jayden.{Guid.NewGuid():N}@example.com";
        var register = await Client.PostAsJsonAsync("/api/auth/register",
            new { email, password = Password, displayName = "Jayden Alcantara" });
        register.EnsureSuccessStatusCode();

        var response = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password = Password });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await response.Content.ReadFromJsonAsync<AuthResult>();
        Assert.That(body, Is.Not.Null);
        Assert.That(body!.AccessToken, Is.Not.Empty);
        Assert.That(body.Email, Is.EqualTo(email));
    }

    [Test]
    [Property("Req", "L2-AUTH-001")]
    public async Task Unknown_email_and_wrong_password_fail_identically_without_revealing_which()
    {
        // AC2: unrecognized email -> generic failure
        var unknown = await Client.PostAsJsonAsync("/api/auth/sign-in",
            new { email = $"nobody.{Guid.NewGuid():N}@example.com", password = "whatever1" });

        // AC3: recognized email, wrong password -> failure
        var email = $"amara.{Guid.NewGuid():N}@example.com";
        await Client.PostAsJsonAsync("/api/auth/register",
            new { email, password = Password, displayName = "Amara Okafor" });
        var wrongPassword = await Client.PostAsJsonAsync("/api/auth/sign-in",
            new { email, password = "WrongPass9" });

        Assert.That(unknown.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(wrongPassword.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var unknownBody = await unknown.Content.ReadAsStringAsync();
        var wrongPasswordBody = await wrongPassword.Content.ReadAsStringAsync();
        Assert.That(wrongPasswordBody, Is.EqualTo(unknownBody),
            "unknown-email and wrong-password responses must be identical so neither confirms the email exists");
    }
}
