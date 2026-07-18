using System.Net;
using System.Net.Http.Json;
using WordUp.Application.Authentication.Onboarding;

namespace WordUp.IntegrationTests.Auth;

// Acceptance Test
// Traces to: L2-AUTH-011 (youth onboarding wizard, skippable)
[TestFixture]
[Property("Req", "L2-AUTH-011")]
public sealed class OnboardingTests : IntegrationTestBase
{
    private async Task<OnboardingStatusResult> GetStatusAsync(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/onboarding");
        Authorize(request, token);
        var response = await Client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<OnboardingStatusResult>())!;
    }

    [Test]
    [Property("Req", "L2-AUTH-011")]
    public async Task Completing_onboarding_sets_display_name_and_marks_it_complete()
    {
        var token = await CreateUserAndGetTokenAsync(Roles.Youth);

        var before = await GetStatusAsync(token);
        Assert.That(before.OnboardingCompleted, Is.False);

        var complete = new HttpRequestMessage(HttpMethod.Post, "/api/onboarding")
        {
            Content = JsonContent.Create(new { displayName = "Jayden", pronouns = "he/him", photoUrl = (string?)null }),
        };
        Authorize(complete, token);
        var completeResponse = await Client.SendAsync(complete);
        Assert.That(completeResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var after = await GetStatusAsync(token);
        Assert.That(after.OnboardingCompleted, Is.True);
        Assert.That(after.DisplayName, Is.EqualTo("Jayden"));
        Assert.That(after.Pronouns, Is.EqualTo("he/him"));
    }

    [Test]
    [Property("Req", "L2-AUTH-011")]
    public async Task Skipping_onboarding_leaves_it_resumable()
    {
        var token = await CreateUserAndGetTokenAsync(Roles.Youth);

        var skip = new HttpRequestMessage(HttpMethod.Post, "/api/onboarding/skip");
        Authorize(skip, token);
        var skipResponse = await Client.SendAsync(skip);
        Assert.That(skipResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Skipping does not complete onboarding — the youth can resume later.
        var status = await GetStatusAsync(token);
        Assert.That(status.OnboardingCompleted, Is.False);
    }
}
