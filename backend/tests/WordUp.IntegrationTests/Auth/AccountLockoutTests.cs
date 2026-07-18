using System.Net.Http.Json;

namespace WordUp.IntegrationTests.Auth;

// Acceptance Test
// Traces to: L2-AUTH-006 (account lockout after repeated failed attempts)
[TestFixture]
[Property("Req", "L2-AUTH-006")]
public sealed class AccountLockoutTests : IntegrationTestBase
{
    private const string Password = "Passw0rd!";

    [Test]
    [Property("Req", "L2-AUTH-006")]
    public async Task Account_locks_at_the_failed_attempt_threshold_and_rejects_even_correct_credentials()
    {
        var email = $"lock.{Guid.NewGuid():N}@example.com";
        await Client.PostAsJsonAsync("/api/auth/register",
            new { email, password = Password, displayName = "Lock Test" });

        System.Net.Http.HttpResponseMessage last = null!;
        for (var i = 0; i < 5; i++)
        {
            last = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password = "WrongPass9" });
        }

        // The threshold attempt locks the account -> 423 Locked.
        Assert.That((int)last.StatusCode, Is.EqualTo(423));

        // While locked, even the correct password is rejected.
        var correct = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password = Password });
        Assert.That((int)correct.StatusCode, Is.EqualTo(423));
    }
}
