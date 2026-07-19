using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Authentication.Auth;
using WordUp.Infrastructure;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.IntegrationTests.Consent;

// Acceptance Test
// Traces to: L2-CONSENT-001 (guardian consent capture), L2-CONSENT-002 / L2-CONSENT-004
//            (server-side per-request participation gate), L2-AUTH-009 (guardian-only-their-own-child)
[TestFixture]
[Property("Req", "L2-CONSENT-001")]
public sealed class ConsentTests : IntegrationTestBase
{
    private sealed record StatusDto(bool ParticipationAllowed);
    private sealed record AccessDto(bool HasCohort, bool ConsentRequired, bool ParticipationAllowed);
    private sealed record ConsentRequestDto(string Channel, string Destination);
    private sealed record PublicConsentDto(string YouthName, string GuardianDestination, DateTime ExpiresAt);
    private sealed record ConsentResultDto(Guid ConsentId, string ReferenceId);

    private async Task<(Guid Id, string Email, string Token)> SeedUserAsync(
        string role,
        Guid? guardianUserId = null,
        Guid? cohortId = null)
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
                CohortId = cohortId,
                PasswordHash = hasher.Hash(password),
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            id = user.Id;
        }

        var response = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password });
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResult>();
        return (id, email, auth!.AccessToken);
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

    private static string ConsentRequestToken(string body)
    {
        var match = Regex.Match(body, @"token=([A-Za-z0-9_-]+)");
        Assert.That(match.Success, Is.True, "The guardian email must contain the secure one-time token.");
        return match.Groups[1].Value;
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

    [Test]
    [Property("Req", "L2-CONSENT-002")]
    [Property("Req", "L2-PLATFORM-011")]
    public async Task Blocked_youth_sends_one_time_guardian_link_and_valid_public_consent_unblocks_access()
    {
        // Traces to: L2-CONSENT-001 AC1-AC3, L2-CONSENT-002 AC2-AC4, L2-PLATFORM-011 AC1
        var guardian = await SeedUserAsync(Roles.Guardian);
        Guid cohortId;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var cohort = new Cohort
            {
                Name = $"Consent cohort {Guid.NewGuid():N}",
                TermStart = DateTime.UtcNow.AddMonths(-1),
                TermEnd = DateTime.UtcNow.AddMonths(5),
            };
            db.Cohorts.Add(cohort);
            await db.SaveChangesAsync();
            cohortId = cohort.Id;
        }
        var youth = await SeedUserAsync(Roles.Youth, guardianUserId: guardian.Id, cohortId: cohortId);

        var before = await GetAsync("/api/me/access", youth.Token);
        var beforeAccess = await before.Content.ReadFromJsonAsync<AccessDto>();
        Assert.Multiple(() =>
        {
            Assert.That(beforeAccess!.HasCohort, Is.True);
            Assert.That(beforeAccess.ConsentRequired, Is.True);
            Assert.That(beforeAccess.ParticipationAllowed, Is.False);
        });

        var request = await PostAsync("/api/consent/request", new { }, youth.Token);
        Assert.That(request.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var delivery = await request.Content.ReadFromJsonAsync<ConsentRequestDto>();
        Assert.Multiple(() =>
        {
            Assert.That(delivery!.Channel, Is.EqualTo("email"));
            Assert.That(delivery.Destination, Does.Contain("***"));
            Assert.That(delivery.Destination, Does.Not.Contain(guardian.Email));
        });

        var email = Factory.Emails.LastTo(guardian.Email);
        Assert.That(email, Is.Not.Null);
        Assert.That(email!.Subject, Does.Contain("consent").IgnoreCase);
        var rawToken = ConsentRequestToken(email.Body);
        Assert.That(await request.Content.ReadAsStringAsync(), Does.Not.Contain(rawToken),
            "The youth-facing response must not disclose the guardian capability token.");

        var detailsResponse = await Client.GetAsync($"/api/public/guardian-consent/{rawToken}");
        Assert.That(detailsResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var details = await detailsResponse.Content.ReadFromJsonAsync<PublicConsentDto>();
        Assert.Multiple(() =>
        {
            Assert.That(details!.YouthName, Is.EqualTo(Roles.Youth));
            Assert.That(details.GuardianDestination, Does.Contain("***"));
            Assert.That(details.ExpiresAt, Is.GreaterThan(DateTime.UtcNow));
        });

        var completion = await Client.PostAsJsonAsync($"/api/public/guardian-consent/{rawToken}", new
        {
            guardianName = "Amara Okafor",
            relationship = "Mother",
            participationConsent = true,
            mediaConsent = true,
            mediaAllPhotosVideo = false,
            mediaGroupOnly = true,
            mediaFirstNameCaptions = true,
            signature = "Amara Okafor",
        });
        Assert.That(completion.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var receipt = await completion.Content.ReadFromJsonAsync<ConsentResultDto>();
        Assert.That(receipt!.ReferenceId, Does.StartWith("GC-"));

        var after = await GetAsync("/api/me/access", youth.Token);
        var afterAccess = await after.Content.ReadFromJsonAsync<AccessDto>();
        Assert.Multiple(() =>
        {
            Assert.That(afterAccess!.ConsentRequired, Is.False);
            Assert.That(afterAccess.ParticipationAllowed, Is.True);
        });

        var reuse = await Client.PostAsJsonAsync($"/api/public/guardian-consent/{rawToken}", new
        {
            guardianName = "Attacker",
            relationship = "Other",
            participationConsent = true,
            signature = "Attacker",
        });
        Assert.That(reuse.StatusCode, Is.EqualTo(HttpStatusCode.Gone));
    }

    [Test]
    [Property("Req", "L2-CONSENT-002")]
    public async Task Consent_request_requires_a_linked_guardian_contact()
    {
        // Traces to: L2-CONSENT-002 AC2
        var youth = await SeedUserAsync(Roles.Youth);

        var request = await PostAsync("/api/consent/request", new { }, youth.Token);

        Assert.That(request.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    [Property("Req", "L2-CONSENT-002")]
    public async Task Every_youth_learning_surface_rechecks_consent_and_the_same_session_unblocks()
    {
        // Traces to: L2-CONSENT-002 AC1/AC3/AC5, L2-CONSENT-004 AC1
        var guardian = await SeedUserAsync(Roles.Guardian);
        var youth = await SeedUserAsync(Roles.Youth, guardianUserId: guardian.Id);
        var protectedUrls = new[]
        {
            "/api/me/curriculum",
            $"/api/me/quizzes/{Guid.NewGuid()}",
            $"/api/me/assignments/{Guid.NewGuid()}",
            $"/api/me/coding-lessons/{Guid.NewGuid()}",
            $"/api/me/reflections/lesson/{Guid.NewGuid()}",
            "/api/me/journey",
            "/api/me/progress",
        };

        foreach (var url in protectedUrls)
        {
            var denied = await GetAsync(url, youth.Token);
            Assert.Multiple(() =>
            {
                Assert.That(denied.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden), url);
                Assert.That(denied.Headers.GetValues("X-WordUp-State").Single(),
                    Is.EqualTo("consent-required"), url);
            });
        }

        var capture = await PostAsync("/api/consent", ConsentBody(youth.Id), guardian.Token);
        Assert.That(capture.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var receipt = await capture.Content.ReadFromJsonAsync<ConsentResultDto>();

        var allowed = await GetAsync("/api/me/curriculum", youth.Token);
        Assert.That(allowed.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var withdrawal = await PostAsync(
            $"/api/consent/{receipt!.ConsentId}/withdraw", new { }, guardian.Token);
        Assert.That(withdrawal.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        var deniedAfterWithdrawal = await GetAsync("/api/me/curriculum", youth.Token);
        Assert.That(deniedAfterWithdrawal.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    [Property("Req", "L2-CONSENT-002")]
    public async Task Guardianless_adult_youth_remains_allowed_on_learning_surfaces()
    {
        // Traces to: L2-CONSENT-002 (minor-only gate)
        var adultYouth = await SeedUserAsync(Roles.Youth);

        var response = await GetAsync("/api/me/curriculum", adultYouth.Token);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}
