// Acceptance Test
// Traces to: L2-GUARDIAN-003 (guardian media-only consent update with participation shown read-only,
//            e-sign record view, contact upsert, and cross-child denial via L2-AUTH-010).
using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Authentication.Auth;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.GuardianPortal;

[TestFixture]
[Property("Req", "L2-GUARDIAN-003")]
public sealed class GuardianConsentTests : IntegrationTestBase
{
    private sealed record ConsentRecordDto(
        Guid ConsentId,
        Guid YouthUserId,
        string GuardianName,
        string Relationship,
        bool ParticipationConsent,
        bool MediaConsent,
        bool MediaAllPhotosVideo,
        bool MediaGroupOnly,
        bool MediaFirstNameCaptions,
        string Method,
        string ReferenceId,
        string? Term,
        DateTime SignedAt,
        DateTime ValidFrom,
        DateTime ValidUntil,
        DateTime? WithdrawnAt);

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, object? body = null, string? token = null)
    {
        var request = new HttpRequestMessage(method, url);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }
        if (token is not null)
        {
            Authorize(request, token);
        }
        return await Client.SendAsync(request);
    }

    private async Task<(Guid Id, string Token)> SeedGuardianAsync()
    {
        var email = $"guardian.{Guid.NewGuid():N}@example.com";
        Guid id;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var user = new User
            {
                Email = email,
                DisplayName = "Guardian",
                Role = Roles.Guardian,
                Status = UserStatus.Active,
                PasswordHash = hasher.Hash("Passw0rd!"),
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            id = user.Id;
        }

        var response = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password = "Passw0rd!" });
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResult>();
        return (id, auth!.AccessToken);
    }

    private async Task<Guid> SeedChildAsync(Guid guardianId)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var child = new User
        {
            Email = $"youth.{Guid.NewGuid():N}@example.com",
            DisplayName = "Child",
            Role = Roles.Youth,
            Status = UserStatus.Active,
            GuardianUserId = guardianId,
            PasswordHash = hasher.Hash("Passw0rd!"),
        };
        db.Users.Add(child);
        await db.SaveChangesAsync();
        return child.Id;
    }

    private static object CaptureBody(Guid youthUserId) => new
    {
        youthUserId,
        guardianName = "Amara Okafor",
        relationship = "Mother",
        participationConsent = true,
        mediaConsent = false,
        mediaAllPhotosVideo = false,
        mediaGroupOnly = false,
        mediaFirstNameCaptions = false,
        signature = "Amara Okafor",
        term = "Autumn 2026",
    };

    [Test]
    [Property("Req", "L2-GUARDIAN-003")]
    public async Task Media_only_update_persists_while_participation_stays_read_only()
    {
        // Traces to: L2-GUARDIAN-003
        var guardian = await SeedGuardianAsync();
        var childId = await SeedChildAsync(guardian.Id);

        // Participation consent already given (media initially off).
        var capture = await SendAsync(HttpMethod.Post, "/api/consent", CaptureBody(childId), guardian.Token);
        Assert.That(capture.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Update media consent only.
        var update = await SendAsync(HttpMethod.Put, $"/api/guardian/children/{childId}/media-consent", body: new
        {
            mediaConsent = true,
            mediaAllPhotosVideo = false,
            mediaGroupOnly = true,
            mediaFirstNameCaptions = true,
        }, token: guardian.Token);
        Assert.That(update.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // The record now reflects the media change and participation remains true (read-only).
        var get = await SendAsync(HttpMethod.Get, $"/api/guardian/children/{childId}/consent", token: guardian.Token);
        Assert.That(get.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var records = await get.Content.ReadFromJsonAsync<List<ConsentRecordDto>>();
        var record = records!.Single();
        Assert.That(record.MediaConsent, Is.True);
        Assert.That(record.MediaGroupOnly, Is.True);
        Assert.That(record.MediaFirstNameCaptions, Is.True);
        Assert.That(record.MediaAllPhotosVideo, Is.False);
        Assert.That(record.ParticipationConsent, Is.True); // untouched by a media-only update
    }

    [Test]
    [Property("Req", "L2-GUARDIAN-003")]
    public async Task E_sign_record_view_returns_signer_date_method_reference_and_validity()
    {
        // Traces to: L2-GUARDIAN-003
        var guardian = await SeedGuardianAsync();
        var childId = await SeedChildAsync(guardian.Id);

        var capture = await SendAsync(HttpMethod.Post, "/api/consent", CaptureBody(childId), guardian.Token);
        Assert.That(capture.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var get = await SendAsync(HttpMethod.Get, $"/api/guardian/children/{childId}/consent", token: guardian.Token);
        Assert.That(get.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var records = await get.Content.ReadFromJsonAsync<List<ConsentRecordDto>>();
        var record = records!.Single();

        Assert.That(record.GuardianName, Is.EqualTo("Amara Okafor"));
        Assert.That(record.Method, Is.EqualTo("typed-e-signature"));
        Assert.That(record.ReferenceId, Is.Not.Empty);
        Assert.That(record.SignedAt, Is.Not.EqualTo(default(DateTime)));
        Assert.That(record.ValidUntil, Is.GreaterThan(record.ValidFrom));
    }

    [Test]
    [Property("Req", "L2-GUARDIAN-003")]
    public async Task Guardian_contact_update_persists_to_user_and_profile()
    {
        // Traces to: L2-GUARDIAN-003
        var guardian = await SeedGuardianAsync();

        var update = await SendAsync(HttpMethod.Put, "/api/guardian/contact", body: new
        {
            displayName = "Amara Okafor",
            email = "amara.okafor@example.com",
            relationship = "Mother",
            phone = "+14165550100",
            emergencyContactName = "John Okafor",
            emergencyContactPhone = "+14165550111",
        }, token: guardian.Token);
        Assert.That(update.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.FirstAsync(u => u.Id == guardian.Id);
        Assert.That(user.DisplayName, Is.EqualTo("Amara Okafor"));
        Assert.That(user.Email, Is.EqualTo("amara.okafor@example.com"));

        var profile = await db.GuardianProfiles.FirstAsync(p => p.GuardianUserId == guardian.Id);
        Assert.That(profile.Relationship, Is.EqualTo("Mother"));
        Assert.That(profile.Phone, Is.EqualTo("+14165550100"));
        Assert.That(profile.EmergencyContactName, Is.EqualTo("John Okafor"));
        Assert.That(profile.EmergencyContactPhone, Is.EqualTo("+14165550111"));
    }

    [Test]
    [Property("Req", "L2-GUARDIAN-003")]
    public async Task A_different_guardian_is_denied_updating_or_viewing_a_childs_consent()
    {
        // Traces to: L2-GUARDIAN-003, L2-AUTH-010
        var owner = await SeedGuardianAsync();
        var childId = await SeedChildAsync(owner.Id);
        await SendAsync(HttpMethod.Post, "/api/consent", CaptureBody(childId), owner.Token);

        var stranger = await SeedGuardianAsync();

        var view = await SendAsync(HttpMethod.Get, $"/api/guardian/children/{childId}/consent", token: stranger.Token);
        Assert.That(view.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        var update = await SendAsync(HttpMethod.Put, $"/api/guardian/children/{childId}/media-consent", body: new
        {
            mediaConsent = true,
            mediaAllPhotosVideo = true,
            mediaGroupOnly = false,
            mediaFirstNameCaptions = false,
        }, token: stranger.Token);
        Assert.That(update.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }
}
