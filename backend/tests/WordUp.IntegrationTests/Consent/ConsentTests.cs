using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Auth;
using WordUp.Domain;
using WordUp.Infrastructure;
using WordUp.Application.Abstractions;

namespace WordUp.IntegrationTests.Consent;

// Acceptance Test
// Traces to: L2-CONSENT-001 (guardian consent capture), L2-CONSENT-002 / L2-CONSENT-004
//            (server-side per-request participation gate), L2-AUTH-009 (guardian-only-their-own-child)
[TestFixture]
[Property("Req", "L2-CONSENT-001")]
public sealed class ConsentTests : IntegrationTestBase
{
    private sealed record StatusDto(bool ParticipationAllowed);

    private async Task<(Guid Id, string Token)> SeedUserAsync(string role, Guid? guardianUserId = null)
    {
        var email = $"{role.ToLowerInvariant()}.{Guid.NewGuid():N}@example.com";
        const string password = "Passw0rd!";
        Guid id;

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var user = new User
            {
                Email = email,
                DisplayName = role,
                Role = role,
                Status = UserStatus.Active,
                GuardianUserId = guardianUserId,
                PasswordHash = hasher.Hash(password),
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            id = user.Id;
        }

        var response = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password });
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResult>();
        return (id, auth!.AccessToken);
    }

    private async Task<HttpResponseMessage> GetAsync(string url, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> PostAsync(string url, object body, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    private static object ConsentBody(Guid youthUserId, bool participation = true) => new
    {
        youthUserId,
        guardianName = "Amara Okafor",
        relationship = "Mother",
        participationConsent = participation,
        mediaConsent = true,
        mediaAllPhotosVideo = false,
        mediaGroupOnly = true,
        mediaFirstNameCaptions = true,
        signature = "Amara Okafor",
        term = "Autumn 2026",
    };

    [Test]
    [Property("Req", "L2-CONSENT-002")]
    public async Task Minor_youth_is_gated_until_guardian_captures_participation_consent()
    {
        // Traces to: L2-CONSENT-001, L2-CONSENT-002, L2-CONSENT-004
        var guardian = await SeedUserAsync(Roles.Guardian);
        var youth = await SeedUserAsync(Roles.Youth, guardianUserId: guardian.Id);

        // (a) Before any consent, the server-side gate denies participation.
        var before = await GetAsync("/api/consent/status", youth.Token);
        Assert.That(before.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var beforeStatus = await before.Content.ReadFromJsonAsync<StatusDto>();
        Assert.That(beforeStatus!.ParticipationAllowed, Is.False);

        // (b) The guardian captures participation consent for their own child.
        var capture = await PostAsync("/api/consent", ConsentBody(youth.Id), guardian.Token);
        Assert.That(capture.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // (c) The gate now allows participation.
        var after = await GetAsync("/api/consent/status", youth.Token);
        var afterStatus = await after.Content.ReadFromJsonAsync<StatusDto>();
        Assert.That(afterStatus!.ParticipationAllowed, Is.True);
    }

    [Test]
    [Property("Req", "L2-CONSENT-001")]
    public async Task Guardian_cannot_capture_consent_for_a_youth_that_is_not_their_child()
    {
        // Traces to: L2-CONSENT-001, L2-AUTH-009
        var guardian = await SeedUserAsync(Roles.Guardian);
        var otherYouth = await SeedUserAsync(Roles.Youth, guardianUserId: Guid.NewGuid());

        var capture = await PostAsync("/api/consent", ConsentBody(otherYouth.Id), guardian.Token);
        Assert.That(capture.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    [Property("Req", "L2-CONSENT-001")]
    public async Task Capturing_consent_without_participation_is_rejected()
    {
        // Traces to: L2-CONSENT-001
        var guardian = await SeedUserAsync(Roles.Guardian);
        var youth = await SeedUserAsync(Roles.Youth, guardianUserId: guardian.Id);

        var capture = await PostAsync("/api/consent", ConsentBody(youth.Id, participation: false), guardian.Token);
        Assert.That(capture.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}
