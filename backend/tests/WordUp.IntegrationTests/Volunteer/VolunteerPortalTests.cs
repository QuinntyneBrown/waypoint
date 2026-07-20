// Acceptance Test
// Traces to: L2-VOLUNTEER-001 (volunteer today view + names-only roster; out-of-scope roster denied 403),
//            L2-VOLUNTEER-002 (upcoming schedule with derived status; empty state)
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Shared.Abstractions;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Volunteer;

[TestFixture]
[Property("Req", "L2-VOLUNTEER-001")]
public sealed class VolunteerPortalTests : IntegrationTestBase
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
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return await Client.SendAsync(request);
    }

    /// <summary>Seeds an active volunteer, signs in, and returns (its id, a bearer token). The id is
    /// what sessions are assigned to via <see cref="Session.VolunteerId"/>.</summary>
    private async Task<(Guid VolunteerId, string Token)> SeedVolunteerAndSignInAsync()
    {
        var email = $"volunteer.{Guid.NewGuid():N}@example.com";
        const string password = "Passw0rd!";
        Guid volunteerId;

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var volunteer = new User
            {
                Email = email,
                DisplayName = "Vera Volunteer",
                Role = Roles.Volunteer,
                Status = UserStatus.Active,
                PasswordHash = hasher.Hash(password),
            };
            db.Users.Add(volunteer);
            await db.SaveChangesAsync();
            volunteerId = volunteer.Id;
        }

        var response = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password });
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return (volunteerId, auth!.AccessToken);
    }

    private async Task<(Guid CohortId, Guid SessionId, List<(Guid Id, string Name)> Youth)> SeedSessionAsync(
        Guid? volunteerId, DateTime startsAt, int youthCount, string title = "Weekly Session")
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var termStart = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc);
        var cohort = new Cohort
        {
            Name = $"Cohort {Guid.NewGuid():N}",
            TermStart = termStart,
            TermEnd = termStart.AddYears(1),
        };
        db.Cohorts.Add(cohort);

        var session = new Session
        {
            CohortId = cohort.Id,
            VolunteerId = volunteerId,
            Title = title,
            StartsAt = startsAt,
            DurationMinutes = 90,
            Location = "Main Hall",
            CreatedByUserId = Guid.NewGuid(),
        };
        db.Sessions.Add(session);

        var youth = new List<(Guid, string)>();
        for (var i = 0; i < youthCount; i++)
        {
            var name = $"Distinct Youth Name {i} {Guid.NewGuid():N}";
            var u = new User
            {
                Email = $"youth.{Guid.NewGuid():N}@example.com",
                DisplayName = name,
                Role = Roles.Youth,
                Status = UserStatus.Active,
                CohortId = cohort.Id,
            };
            db.Users.Add(u);
            youth.Add((u.Id, name));
        }

        await db.SaveChangesAsync();
        return (cohort.Id, session.Id, youth);
    }

    private async Task SeedAttendanceAsync(Guid sessionId, Guid youthUserId, Guid recordedBy, AttendanceStatus status)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.AttendanceRecords.Add(new AttendanceRecord
        {
            SessionId = sessionId,
            YouthUserId = youthUserId,
            Status = status,
            RecordedByUserId = recordedBy,
            RecordedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();
    }

    private static DateTime NoonTodayUtc() => DateTime.UtcNow.Date.AddHours(12);

    [Test]
    [Property("Req", "L2-VOLUNTEER-001")]
    public async Task Today_view_shows_only_the_assigned_session_with_roster_count_and_no_youth_records()
    {
        // Traces to: L2-VOLUNTEER-001
        var (volunteerId, token) = await SeedVolunteerAndSignInAsync();
        var (_, mySessionId, myYouth) = await SeedSessionAsync(volunteerId, NoonTodayUtc(), youthCount: 3, title: "My Today Session");

        // A different volunteer's session today must NOT leak into this volunteer's view.
        var (otherId, _) = await SeedVolunteerAndSignInAsync();
        await SeedSessionAsync(otherId, NoonTodayUtc(), youthCount: 5, title: "Other Session");

        var response = await SendAsync(HttpMethod.Get, "/api/volunteer/today", token: token);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var today = await response.Content.ReadFromJsonAsync<TodayDto>();
        Assert.That(today, Is.Not.Null);
        Assert.That(today!.SessionId, Is.EqualTo(mySessionId));
        Assert.That(today.Title, Is.EqualTo("My Today Session"));
        Assert.That(today.DurationMinutes, Is.EqualTo(90));
        Assert.That(today.Location, Is.EqualTo("Main Hall"));
        Assert.That(today.RosterCount, Is.EqualTo(3));

        // No youth records: the today payload must not carry any youth's name.
        var raw = await response.Content.ReadAsStringAsync();
        foreach (var (_, name) in myYouth)
        {
            Assert.That(raw.Contains(name), Is.False, "today view must not expose youth records");
        }
    }

    [Test]
    [Property("Req", "L2-VOLUNTEER-001")]
    public async Task Today_view_is_null_when_the_volunteer_has_no_session_today()
    {
        // Traces to: L2-VOLUNTEER-001
        var (volunteerId, token) = await SeedVolunteerAndSignInAsync();
        // Only a future session — nothing today.
        await SeedSessionAsync(volunteerId, DateTime.UtcNow.AddDays(3), youthCount: 2);

        var response = await SendAsync(HttpMethod.Get, "/api/volunteer/today", token: token);

        // No session today → no resource to return (204 No Content).
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    [Property("Req", "L2-VOLUNTEER-001")]
    public async Task Roster_returns_youth_names_and_marks_only_with_no_progress_or_flag_data()
    {
        // Traces to: L2-VOLUNTEER-001
        var (volunteerId, token) = await SeedVolunteerAndSignInAsync();
        var (_, sessionId, youth) = await SeedSessionAsync(volunteerId, NoonTodayUtc(), youthCount: 3);

        // One youth already marked Present so the Mark field is exercised.
        await SeedAttendanceAsync(sessionId, youth[0].Id, volunteerId, AttendanceStatus.Present);

        var response = await SendAsync(HttpMethod.Get, $"/api/volunteer/sessions/{sessionId}/roster", token: token);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var roster = await response.Content.ReadFromJsonAsync<List<RosterEntryDto>>();
        Assert.That(roster, Is.Not.Null);
        Assert.That(roster!, Has.Count.EqualTo(3));

        // Names are present and sorted; marks reflect current attendance (null when unmarked).
        var expectedNames = youth.Select(y => y.Name).OrderBy(n => n, StringComparer.Ordinal).ToList();
        Assert.That(roster!.Select(r => r.Name), Is.EqualTo(expectedNames));
        var marked = roster!.Single(r => r.YouthUserId == youth[0].Id);
        Assert.That(marked.Mark, Is.EqualTo("Present"));
        Assert.That(roster!.Where(r => r.YouthUserId != youth[0].Id).All(r => r.Mark is null), Is.True);

        // No progress / at-risk / flag / note / safeguarding fields are exposed (AC2).
        var raw = await response.Content.ReadAsStringAsync();
        foreach (var forbidden in new[] { "progress", "atrisk", "at-risk", "flag", "note", "safeguard" })
        {
            Assert.That(raw.Contains(forbidden, StringComparison.OrdinalIgnoreCase), Is.False,
                $"roster must not expose '{forbidden}'");
        }
    }

    [Test]
    [Property("Req", "L2-VOLUNTEER-001")]
    public async Task Requesting_a_roster_for_an_unassigned_session_is_forbidden()
    {
        // Traces to: L2-VOLUNTEER-001
        var (_, tokenA) = await SeedVolunteerAndSignInAsync();
        var (volunteerB, _) = await SeedVolunteerAndSignInAsync();

        // Session belongs to volunteer B; volunteer A must not be able to read its roster.
        var (_, sessionB, _) = await SeedSessionAsync(volunteerB, NoonTodayUtc(), youthCount: 2);

        var response = await SendAsync(HttpMethod.Get, $"/api/volunteer/sessions/{sessionB}/roster", token: tokenA);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    [Property("Req", "L2-VOLUNTEER-001")]
    public async Task Saving_attendance_for_an_unassigned_session_is_forbidden()
    {
        // Traces to: L2-VOLUNTEER-001 AC4
        var (_, tokenA) = await SeedVolunteerAndSignInAsync();
        var (volunteerB, _) = await SeedVolunteerAndSignInAsync();
        var (_, sessionB, youth) = await SeedSessionAsync(volunteerB, NoonTodayUtc(), youthCount: 1);

        var response = await SendAsync(HttpMethod.Post, "/api/attendance", new
        {
            sessionId = sessionB,
            marks = new[] { new { youthId = youth[0].Id, status = "Present", note = (string?)null } },
        }, tokenA);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.That(db.AttendanceRecords.Any(r => r.SessionId == sessionB), Is.False,
            "a volunteer must not be able to write another volunteer's session");
    }

    [Test]
    [Property("Req", "L2-VOLUNTEER-001")]
    public async Task Volunteer_is_denied_the_staff_attendance_record_and_history_resources()
    {
        // Traces to: L2-VOLUNTEER-001 AC4
        var (volunteerId, token) = await SeedVolunteerAndSignInAsync();
        var (_, sessionId, _) = await SeedSessionAsync(volunteerId, NoonTodayUtc(), youthCount: 1);

        var records = await SendAsync(HttpMethod.Get, $"/api/attendance/{sessionId}", token: token);
        var history = await SendAsync(
            HttpMethod.Get,
            "/api/attendance/history/by-session",
            token: token);

        Assert.Multiple(() =>
        {
            Assert.That(records.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(history.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        });
    }

    [Test]
    [Property("Req", "L2-VOLUNTEER-002")]
    public async Task Schedule_lists_only_this_volunteers_upcoming_assigned_sessions_in_order()
    {
        // Traces to: L2-VOLUNTEER-002
        var (volunteerId, token) = await SeedVolunteerAndSignInAsync();

        var (_, laterId, _) = await SeedSessionAsync(volunteerId, DateTime.UtcNow.AddDays(10), youthCount: 1, title: "Later");
        var (_, soonerId, _) = await SeedSessionAsync(volunteerId, DateTime.UtcNow.AddDays(3), youthCount: 1, title: "Sooner");

        // A past session (not upcoming) and another volunteer's future session must be excluded.
        await SeedSessionAsync(volunteerId, DateTime.UtcNow.AddDays(-2), youthCount: 1, title: "Past");
        var (otherId, _) = await SeedVolunteerAndSignInAsync();
        await SeedSessionAsync(otherId, DateTime.UtcNow.AddDays(5), youthCount: 1, title: "Other Volunteer");

        var response = await SendAsync(HttpMethod.Get, "/api/volunteer/schedule", token: token);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var schedule = await response.Content.ReadFromJsonAsync<List<ScheduleItemDto>>();
        Assert.That(schedule, Is.Not.Null);
        Assert.That(schedule!.Select(s => s.SessionId), Is.EqualTo(new[] { soonerId, laterId }));
        Assert.That(schedule!.All(s => s.Status == "upcoming"), Is.True);
    }

    [Test]
    [Property("Req", "L2-VOLUNTEER-002")]
    public async Task Schedule_marks_a_session_with_saved_attendance_as_attendance_taken()
    {
        // Traces to: L2-VOLUNTEER-002
        var (volunteerId, token) = await SeedVolunteerAndSignInAsync();
        var (_, sessionId, youth) = await SeedSessionAsync(volunteerId, DateTime.UtcNow.AddDays(4), youthCount: 1);
        await SeedAttendanceAsync(sessionId, youth[0].Id, volunteerId, AttendanceStatus.Present);

        var response = await SendAsync(HttpMethod.Get, "/api/volunteer/schedule", token: token);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var schedule = await response.Content.ReadFromJsonAsync<List<ScheduleItemDto>>();
        var item = schedule!.Single(s => s.SessionId == sessionId);
        Assert.That(item.Status, Is.EqualTo("attendance-taken"));
    }

    [Test]
    [Property("Req", "L2-VOLUNTEER-002")]
    public async Task Schedule_with_no_assigned_sessions_returns_an_empty_list()
    {
        // Traces to: L2-VOLUNTEER-002
        var (_, token) = await SeedVolunteerAndSignInAsync();

        var response = await SendAsync(HttpMethod.Get, "/api/volunteer/schedule", token: token);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var schedule = await response.Content.ReadFromJsonAsync<List<ScheduleItemDto>>();
        Assert.That(schedule, Is.Not.Null);
        Assert.That(schedule!, Is.Empty);
    }

    private sealed record TokenResponse(string AccessToken);

    private sealed record TodayDto(
        Guid SessionId, string Title, DateTime StartsAt, int? DurationMinutes, string? Location, int RosterCount);

    private sealed record RosterEntryDto(Guid YouthUserId, string Name, string? Mark);

    private sealed record ScheduleItemDto(
        Guid SessionId, string Title, DateTime StartsAt, int? DurationMinutes, string? Location, string Status);
}
