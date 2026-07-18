// Acceptance Test
// Traces to: L2-GUARDIAN-001 (guardian read-only child aggregate: progress/attendance/badges/schedule)
//            and its cross-child denial via L2-AUTH-010.
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Authentication.Auth;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.GuardianPortal;

[TestFixture]
[Property("Req", "L2-GUARDIAN-001")]
public sealed class GuardianProgressTests : IntegrationTestBase
{
    private sealed record ChildProgressViewDto(
        Guid ChildId,
        string ChildName,
        double OverallPercent,
        List<ModuleStatusDto> Modules,
        AttendanceDto Attendance,
        List<BadgeDto> EarnedBadges,
        List<BadgeDto> LockedBadges,
        List<UpcomingSessionDto> UpcomingSessions);

    private sealed record ModuleStatusDto(
        Guid ModuleId, string Name, int Order, string Status, double CompletionPercent, bool Locked, string? UnlockCondition);

    private sealed record AttendanceDto(int Present, int Late, int Excused, int Absent, int Total, double Rate);

    private sealed record BadgeDto(Guid BadgeId, string Name, string Icon, string? Description);

    private sealed record UpcomingSessionDto(Guid SessionId, string Title, DateTime StartsAt, string? Location);

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

    private async Task<Guid> SeedUserAsync(string role, Guid? guardianUserId = null, Guid? cohortId = null)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var user = new User
        {
            Email = $"{role.ToLowerInvariant()}.{Guid.NewGuid():N}@example.com",
            DisplayName = role,
            Role = role,
            Status = UserStatus.Active,
            GuardianUserId = guardianUserId,
            CohortId = cohortId,
            PasswordHash = hasher.Hash("Passw0rd!"),
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user.Id;
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

    private async Task<(Guid CohortId, Guid SessionId, Guid BadgeId)> SeedCohortSessionAttendanceBadgeAsync(Guid childId)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var termStart = new DateTime(2026, 9, 1, 18, 0, 0, DateTimeKind.Utc);
        var cohort = new Cohort
        {
            Name = $"Cohort {Guid.NewGuid():N}",
            TermStart = termStart,
            TermEnd = termStart.AddYears(1),
        };
        db.Cohorts.Add(cohort);

        // Attach the child to this cohort.
        var child = db.Users.First(u => u.Id == childId);
        child.CohortId = cohort.Id;

        // A future session -> should surface in the upcoming schedule.
        var session = new Session
        {
            CohortId = cohort.Id,
            Title = "Upcoming Weekly Session",
            StartsAt = DateTime.UtcNow.AddDays(7),
            CreatedByUserId = Guid.NewGuid(),
        };
        db.Sessions.Add(session);

        // One present attendance mark for the child.
        db.AttendanceRecords.Add(new AttendanceRecord
        {
            SessionId = Guid.NewGuid(),
            YouthUserId = childId,
            Status = AttendanceStatus.Present,
            RecordedByUserId = Guid.NewGuid(),
        });

        // One earned badge.
        var badge = new Badge { Name = "First Steps", Icon = "star" };
        db.Badges.Add(badge);
        db.BadgeAwards.Add(new BadgeAward { BadgeId = badge.Id, YouthUserId = childId });

        await db.SaveChangesAsync();
        return (cohort.Id, session.Id, badge.Id);
    }

    [Test]
    [Property("Req", "L2-GUARDIAN-001")]
    public async Task Guardian_sees_their_own_childs_aggregate_progress()
    {
        // Traces to: L2-GUARDIAN-001
        var guardian = await SeedGuardianAsync();
        var childId = await SeedUserAsync(Roles.Youth, guardianUserId: guardian.Id);
        var (_, sessionId, badgeId) = await SeedCohortSessionAttendanceBadgeAsync(childId);

        var response = await SendAsync(HttpMethod.Get, $"/api/guardian/children/{childId}/progress", token: guardian.Token);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var view = await response.Content.ReadFromJsonAsync<ChildProgressViewDto>();
        Assert.That(view, Is.Not.Null);
        Assert.That(view!.ChildId, Is.EqualTo(childId));
        Assert.That(view.Attendance.Present, Is.EqualTo(1));
        Assert.That(view.Attendance.Total, Is.EqualTo(1));
        Assert.That(view.UpcomingSessions.Any(s => s.SessionId == sessionId), Is.True);
        Assert.That(view.EarnedBadges.Any(b => b.BadgeId == badgeId), Is.True);
        Assert.That(view.LockedBadges.Any(b => b.BadgeId == badgeId), Is.False);
    }

    [Test]
    [Property("Req", "L2-GUARDIAN-001")]
    public async Task A_different_guardian_is_denied_another_guardians_child_progress()
    {
        // Traces to: L2-GUARDIAN-001, L2-AUTH-010
        var owner = await SeedGuardianAsync();
        var childId = await SeedUserAsync(Roles.Youth, guardianUserId: owner.Id);

        var stranger = await SeedGuardianAsync();
        var response = await SendAsync(HttpMethod.Get, $"/api/guardian/children/{childId}/progress", token: stranger.Token);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }
}
