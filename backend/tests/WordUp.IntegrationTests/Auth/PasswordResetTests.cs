using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace WordUp.IntegrationTests.Auth;

// Acceptance Test
// Traces to: L2-AUTH-005 (password reset: no enumeration, single-use link, session invalidation, confirm match)
[TestFixture]
[Property("Req", "L2-AUTH-005")]
public sealed class PasswordResetTests : IntegrationTestBase
{
    private Task<HttpResponseMessage> RequestReset(string email) =>
        Client.PostAsJsonAsync("/api/auth/password-reset/request", new { email });

    private Task<HttpResponseMessage> Confirm(string token, string newPassword, string confirmPassword) =>
        Client.PostAsJsonAsync("/api/auth/password-reset/confirm",
            new { token, newPassword, confirmPassword });

    private string TokenFor(string email)
    {
        var sent = Factory.Emails.LastTo(email);
        Assert.That(sent, Is.Not.Null, "a reset email should have been sent");
        return Regex.Match(sent!.Body, @"token=(\S+)").Groups[1].Value;
    }

    private async Task<string> RegisterAsync(string email, string password)
    {
        var response = await Client.PostAsJsonAsync("/api/auth/register",
            new { email, password, displayName = "Reset Tester" });
        response.EnsureSuccessStatusCode();
        return email;
    }

    // Traces to: L2-AUTH-005 (AC1 — no account enumeration)
    [Test]
    [Property("Req", "L2-AUTH-005")]
    public async Task Requesting_a_reset_returns_the_same_status_for_existing_and_unknown_emails()
    {
        var existing = $"exists.{Guid.NewGuid():N}@example.com";
        await RegisterAsync(existing, "Passw0rd!");
        var unknown = $"unknown.{Guid.NewGuid():N}@example.com";

        var existingResponse = await RequestReset(existing);
        var unknownResponse = await RequestReset(unknown);

        Assert.That(existingResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That(unknownResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    // Traces to: L2-AUTH-005 (AC2/AC3 — reset link works, old password stops working)
    [Test]
    [Property("Req", "L2-AUTH-005")]
    public async Task Full_reset_flow_lets_the_user_sign_in_with_the_new_password_only()
    {
        var email = $"flow.{Guid.NewGuid():N}@example.com";
        const string oldPassword = "Passw0rd!";
        const string newPassword = "N3wPassw0rd!";
        await RegisterAsync(email, oldPassword);

        var request = await RequestReset(email);
        Assert.That(request.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var confirm = await Confirm(TokenFor(email), newPassword, newPassword);
        Assert.That(confirm.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var withNew = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password = newPassword });
        Assert.That(withNew.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var withOld = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password = oldPassword });
        Assert.That(withOld.StatusCode, Is.Not.EqualTo(HttpStatusCode.OK));
    }

    // Traces to: L2-AUTH-005 (single-use reset link)
    [Test]
    [Property("Req", "L2-AUTH-005")]
    public async Task Reusing_a_reset_token_a_second_time_is_gone()
    {
        var email = $"reuse.{Guid.NewGuid():N}@example.com";
        await RegisterAsync(email, "Passw0rd!");

        await RequestReset(email);
        var token = TokenFor(email);

        var first = await Confirm(token, "N3wPassw0rd!", "N3wPassw0rd!");
        Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var second = await Confirm(token, "An0therPass!", "An0therPass!");
        Assert.That((int)second.StatusCode, Is.EqualTo(410));
    }

    // Traces to: L2-AUTH-005 (AC5 — new/confirm password must match)
    [Test]
    [Property("Req", "L2-AUTH-005")]
    public async Task Confirming_with_mismatched_passwords_is_rejected()
    {
        var email = $"mismatch.{Guid.NewGuid():N}@example.com";
        await RegisterAsync(email, "Passw0rd!");

        await RequestReset(email);
        var token = TokenFor(email);

        var confirm = await Confirm(token, "N3wPassw0rd!", "Diff3rent!");
        Assert.That(confirm.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}
