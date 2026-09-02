// Acceptance Test
// Traces to: L2-PLATFORM-002
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Authentication.Auth;
using Waypoint.Infrastructure;

namespace Waypoint.IntegrationTests.Platform;

[TestFixture]
[Property("Req", "L2-PLATFORM-002")]
public sealed class ProfileTests : IntegrationTestBase
{
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

    private async Task<(Guid Id, string Email)> SeedUserAsync(string role)
    {
        var email = $"seed.{role.ToLowerInvariant()}.{Guid.NewGuid():N}@example.com";
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var user = new User
        {
            Email = email,
            DisplayName = $"Seed {role}",
            Role = role,
            Status = UserStatus.Active,
            PasswordHash = hasher.Hash("Passw0rd!"),
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return (user.Id, email);
    }

    private async Task<string> SignInAsync(string email, string password = "Passw0rd!")
    {
        var response = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password });
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResult>();
        return auth!.AccessToken;
    }

    private async Task<HttpResponseMessage> UploadPhotoAsync(
        string token,
        string fileName,
        string contentType,
        byte[] content)
    {
        var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(content);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(file, "file", fileName);
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/me/profile/photo") { Content = form };
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    [Test]
    [Property("Req", "L2-PLATFORM-002")]
    public async Task A_user_updates_their_display_name_and_email()
    {
        // Traces to: L2-PLATFORM-002
        var user = await SeedUserAsync(Roles.Youth);
        var token = await SignInAsync(user.Email);

        var newEmail = $"updated.{Guid.NewGuid():N}@example.com";
        var response = await SendAsync(HttpMethod.Put, "/api/me/profile", body: new
        {
            displayName = "Updated Name",
            email = newEmail,
            pronouns = "they/them",
            photoUrl = "https://example.com/me.png",
        }, token: token);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var stored = db.Users.Single(u => u.Id == user.Id);
        Assert.That(stored.DisplayName, Is.EqualTo("Updated Name"));
        Assert.That(stored.Email, Is.EqualTo(newEmail));
        Assert.That(stored.Pronouns, Is.EqualTo("they/them"));
    }

    [Test]
    [Property("Req", "L2-NFR-001")]
    public async Task Profile_photo_is_signature_validated_private_replaceable_and_removable()
    {
        // Traces to: L2-PLATFORM-002 AC3, L2-NFR-001 AC4
        var owner = await SeedUserAsync(Roles.Youth);
        var ownerToken = await SignInAsync(owner.Email);
        var otherToken = await CreateUserAndGetTokenAsync(Roles.Youth);

        var disguised = await UploadPhotoAsync(
            ownerToken,
            "not-an-image.png",
            "image/png",
            "<script>alert(1)</script>"u8.ToArray());
        Assert.That(disguised.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var png = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=");
        var uploaded = await UploadPhotoAsync(ownerToken, "me.png", "image/png", png);
        Assert.That(uploaded.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var first = await uploaded.Content.ReadFromJsonAsync<ProfilePhotoDto>();

        var anonymous = await Client.GetAsync(first!.Url);
        Assert.That(anonymous.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        var forbidden = await SendAsync(HttpMethod.Get, first.Url, token: otherToken);
        Assert.That(forbidden.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        var served = await SendAsync(HttpMethod.Get, first.Url, token: ownerToken);
        Assert.Multiple(() =>
        {
            Assert.That(served.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(served.Content.Headers.ContentType!.MediaType, Is.EqualTo("image/png"));
            Assert.That(served.Headers.GetValues("X-Content-Type-Options").Single(), Is.EqualTo("nosniff"));
        });

        var replaced = await UploadPhotoAsync(ownerToken, "replacement.png", "image/png", png);
        var second = await replaced.Content.ReadFromJsonAsync<ProfilePhotoDto>();
        Assert.That((await SendAsync(HttpMethod.Get, first.Url, token: ownerToken)).StatusCode,
            Is.EqualTo(HttpStatusCode.NotFound));
        var profile = await SendAsync(HttpMethod.Get, "/api/me/profile", token: ownerToken);
        Assert.That((await profile.Content.ReadFromJsonAsync<MyProfileDto>())!.PhotoUrl,
            Is.EqualTo(second!.Url));

        var removed = await SendAsync(HttpMethod.Delete, "/api/me/profile/photo", token: ownerToken);
        Assert.That(removed.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That((await SendAsync(HttpMethod.Get, second.Url, token: ownerToken)).StatusCode,
            Is.EqualTo(HttpStatusCode.NotFound));
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.That(db.Users.Single(item => item.Id == owner.Id).PhotoUrl, Is.Null);
    }

    [Test]
    [Property("Req", "L2-CONSENT-004")]
    [Property("Req", "L2-NFR-001")]
    public async Task Linked_minor_profile_photo_follows_individual_media_consent_live()
    {
        // Traces to: L2-PLATFORM-002 AC3, L2-CONSENT-004 AC3, L2-NFR-001 AC4
        var guardianEmail = $"guardian.{Guid.NewGuid():N}@example.com";
        var youthEmail = $"minor.{Guid.NewGuid():N}@example.com";
        Guid youthId;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var guardian = new User
            {
                Email = guardianEmail,
                DisplayName = "Guardian",
                Role = Roles.Guardian,
                Status = UserStatus.Active,
                PasswordHash = hasher.Hash("Passw0rd!"),
            };
            var youth = new User
            {
                Email = youthEmail,
                DisplayName = "Minor",
                Role = Roles.Youth,
                Status = UserStatus.Active,
                GuardianUserId = guardian.Id,
                PasswordHash = hasher.Hash("Passw0rd!"),
            };
            db.Users.AddRange(guardian, youth);
            db.GuardianConsents.Add(new GuardianConsent
            {
                YouthUserId = youth.Id,
                GuardianUserId = guardian.Id,
                GuardianName = guardian.DisplayName,
                Relationship = "Parent",
                ParticipationConsent = true,
                MediaConsent = true,
                MediaGroupOnly = true,
                Signature = guardian.DisplayName,
                ReferenceId = $"PROFILE-{Guid.NewGuid():N}",
                SignedAt = DateTime.UtcNow.AddDays(-1),
                ValidFrom = DateTime.UtcNow.AddDays(-1),
                ValidUntil = DateTime.UtcNow.AddMonths(6),
            });
            await db.SaveChangesAsync();
            youthId = youth.Id;
        }
        var youthToken = await SignInAsync(youthEmail);
        var guardianToken = await SignInAsync(guardianEmail);
        var png = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=");

        var groupOnly = await UploadPhotoAsync(youthToken, "minor.png", "image/png", png);
        Assert.Multiple(() =>
        {
            Assert.That(groupOnly.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(groupOnly.Headers.GetValues("X-Waypoint-State").Single(),
                Is.EqualTo("media-consent-required"));
        });

        var grant = await SendAsync(
            HttpMethod.Put,
            $"/api/guardian/children/{youthId}/media-consent",
            new
            {
                mediaConsent = true,
                mediaAllPhotosVideo = true,
                mediaGroupOnly = false,
                mediaFirstNameCaptions = false,
            },
            guardianToken);
        Assert.That(grant.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        var uploaded = await UploadPhotoAsync(youthToken, "minor.png", "image/png", png);
        var photo = await uploaded.Content.ReadFromJsonAsync<ProfilePhotoDto>();
        Assert.That(uploaded.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var withdrawMedia = await SendAsync(
            HttpMethod.Put,
            $"/api/guardian/children/{youthId}/media-consent",
            new
            {
                mediaConsent = false,
                mediaAllPhotosVideo = false,
                mediaGroupOnly = false,
                mediaFirstNameCaptions = false,
            },
            guardianToken);
        Assert.That(withdrawMedia.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That((await SendAsync(HttpMethod.Get, photo!.Url, token: youthToken)).StatusCode,
            Is.EqualTo(HttpStatusCode.Forbidden));
        var profile = await SendAsync(HttpMethod.Get, "/api/me/profile", token: youthToken);
        var view = await profile.Content.ReadFromJsonAsync<MyProfileDto>();
        Assert.Multiple(() =>
        {
            Assert.That(view!.PhotoUrl, Is.Null);
            Assert.That(view.HasPhoto, Is.True, "the retained photo can still be explicitly removed");
        });
    }

    [Test]
    [Property("Req", "L2-PLATFORM-002")]
    public async Task Changing_the_password_requires_the_correct_current_password_and_then_the_new_password_signs_in()
    {
        // Traces to: L2-PLATFORM-002
        var user = await SeedUserAsync(Roles.Youth);
        var token = await SignInAsync(user.Email);

        // A wrong current password is rejected.
        var wrong = await SendAsync(HttpMethod.Post, "/api/me/change-password", body: new
        {
            currentPassword = "Wr0ngPass!",
            newPassword = "NewPassw0rd!",
            confirmPassword = "NewPassw0rd!",
        }, token: token);
        Assert.That(wrong.IsSuccessStatusCode, Is.False);

        // The correct current password succeeds.
        var changed = await SendAsync(HttpMethod.Post, "/api/me/change-password", body: new
        {
            currentPassword = "Passw0rd!",
            newPassword = "NewPassw0rd!",
            confirmPassword = "NewPassw0rd!",
        }, token: token);
        Assert.That(changed.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Signing in with the NEW password works.
        var newToken = await SignInAsync(user.Email, "NewPassw0rd!");
        Assert.That(newToken, Is.Not.Empty);
    }

    private sealed record ProfilePhotoDto(Guid Id, string FileName, string ContentType, long SizeBytes, string Url);
    private sealed record MyProfileDto(
        string DisplayName,
        string Email,
        string? Pronouns,
        string? PhotoUrl,
        bool HasPhoto);
}
