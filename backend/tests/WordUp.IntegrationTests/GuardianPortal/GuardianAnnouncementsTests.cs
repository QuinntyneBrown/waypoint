// Acceptance Test
// Traces to: L2-GUARDIAN-002 (guardian announcements: guardian/child-cohort targeting, unread
//            indicators, mark-all-read clears unread).
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Authentication.Auth;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.GuardianPortal;

[TestFixture]
[Property("Req", "L2-GUARDIAN-002")]
public sealed class GuardianAnnouncementsTests : IntegrationTestBase
{
    private sealed record AnnouncementDto(Guid Id, string Title, string Message, DateTime? SentAt, bool Read);

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

    private async Task<Guid> SeedChildInNewCohortAsync(Guid guardianId)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var termStart = new DateTime(2026, 9, 1, 18, 0, 0, DateTimeKind.Utc);
        var cohort = new Cohort { Name = $"Cohort {Guid.NewGuid():N}", TermStart = termStart, TermEnd = termStart.AddYears(1) };
        db.Cohorts.Add(cohort);

        var child = new User
        {
            Email = $"youth.{Guid.NewGuid():N}@example.com",
            DisplayName = "Child",
            Role = Roles.Youth,
            Status = UserStatus.Active,
            GuardianUserId = guardianId,
            CohortId = cohort.Id,
            PasswordHash = hasher.Hash("Passw0rd!"),
        };
        db.Users.Add(child);
        await db.SaveChangesAsync();
        return cohort.Id;
    }

    private async Task<Guid> SeedAnnouncementAsync(
        string title, string audienceScope, string roleTargeting, Guid? cohortId = null, bool sent = true)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var announcement = new Announcement
        {
            Title = title,
            Message = $"{title} body",
            AudienceScope = audienceScope,
            CohortId = cohortId,
            RoleTargeting = roleTargeting,
            SentAt = sent ? DateTime.UtcNow : null,
            CreatedByUserId = Guid.NewGuid(),
        };
        db.Announcements.Add(announcement);
        await db.SaveChangesAsync();
        return announcement.Id;
    }

    [Test]
    [Property("Req", "L2-GUARDIAN-002")]
    public async Task Guardian_sees_a_guardians_targeted_announcement_and_mark_all_read_clears_unread()
    {
        // Traces to: L2-GUARDIAN-002
        var guardian = await SeedGuardianAsync();
        await SeedChildInNewCohortAsync(guardian.Id);

        var title = $"Guardians notice {Guid.NewGuid():N}";
        var announcementId = await SeedAnnouncementAsync(title, audienceScope: "org", roleTargeting: "Guardians");

        // Appears as unread.
        var first = await SendAsync(HttpMethod.Get, "/api/guardian/announcements", token: guardian.Token);
        Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var beforeList = await first.Content.ReadFromJsonAsync<List<AnnouncementDto>>();
        var before = beforeList!.SingleOrDefault(a => a.Id == announcementId);
        Assert.That(before, Is.Not.Null, "Guardians-targeted announcement should be visible to the guardian.");
        Assert.That(before!.Read, Is.False);

        // Mark all read.
        var markAll = await SendAsync(HttpMethod.Post, "/api/guardian/announcements/read-all", token: guardian.Token);
        Assert.That(markAll.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Now read.
        var second = await SendAsync(HttpMethod.Get, "/api/guardian/announcements", token: guardian.Token);
        var afterList = await second.Content.ReadFromJsonAsync<List<AnnouncementDto>>();
        var after = afterList!.Single(a => a.Id == announcementId);
        Assert.That(after.Read, Is.True);
    }

    [Test]
    [Property("Req", "L2-GUARDIAN-002")]
    public async Task Guardian_sees_child_cohort_announcements_but_not_youth_only_targeting()
    {
        // Traces to: L2-GUARDIAN-002
        var guardian = await SeedGuardianAsync();
        var childCohortId = await SeedChildInNewCohortAsync(guardian.Id);

        var cohortTitle = $"Cohort notice {Guid.NewGuid():N}";
        var cohortAnnouncementId = await SeedAnnouncementAsync(
            cohortTitle, audienceScope: "cohort", roleTargeting: "YouthGuardians", cohortId: childCohortId);

        var youthOnlyTitle = $"Youth only {Guid.NewGuid():N}";
        var youthOnlyAnnouncementId = await SeedAnnouncementAsync(
            youthOnlyTitle, audienceScope: "org", roleTargeting: "Youth");

        var response = await SendAsync(HttpMethod.Get, "/api/guardian/announcements", token: guardian.Token);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var list = await response.Content.ReadFromJsonAsync<List<AnnouncementDto>>();

        // Cohort-scoped (resolved via the linked child's cohort) is visible.
        Assert.That(list!.Any(a => a.Id == cohortAnnouncementId), Is.True);
        // Youth-only targeting is filtered out for a guardian.
        Assert.That(list!.Any(a => a.Id == youthOnlyAnnouncementId), Is.False);
    }
}
