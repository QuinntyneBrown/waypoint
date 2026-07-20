using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Authentication.Auth;
using WordUp.Application.Engagement.Attendance;
using WordUp.Application.Engagement.Sessions;
using WordUp.Application.Shared.Abstractions;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Engagement;

// Acceptance Test
// Traces to: L2-ENGAGE-001..004, L2-AUTH-010, L2-REPORT-003
[TestFixture]
[Property("Req", "L2-ENGAGE-001")]
public sealed class MentorCalendarAttendanceTests : IntegrationTestBase
{
    private sealed record Scenario(
        User Mentor,
        User OtherMentor,
        Cohort Cohort,
        Cohort OtherCohort,
        User YouthA,
        User YouthB,
        User OtherYouth,
        Session Session,
        Session OtherSession);

    [Test]
    public async Task Mentor_calendar_is_named_scoped_filterable_and_schedules_recurrence()
    {
        // Traces to: L2-ENGAGE-001 AC1..AC4, L2-AUTH-010
        var scenario = await SeedScenarioAsync();
        var token = await SignInAsync(scenario.Mentor.Email);

        var optionsResponse = await SendAsync(HttpMethod.Get, "/api/sessions/options", token);
        var options = await optionsResponse.Content.ReadFromJsonAsync<SessionSchedulingOptions>();
        Assert.Multiple(() =>
        {
            Assert.That(options!.Cohorts.Select(item => item.CohortId),
                Is.EquivalentTo(new[] { scenario.Cohort.Id }));
            Assert.That(options.Mentors.Select(item => item.MentorId),
                Is.EquivalentTo(new[] { scenario.Mentor.Id }));
        });

        var calendar = await SendAsync(
            HttpMethod.Get,
            $"/api/sessions?from={DateTime.UtcNow.AddDays(-7):O}&to={DateTime.UtcNow.AddDays(30):O}",
            token);
        var sessions = await calendar.Content.ReadFromJsonAsync<List<SessionView>>();
        Assert.Multiple(() =>
        {
            Assert.That(sessions!.Select(item => item.Id), Does.Contain(scenario.Session.Id));
            Assert.That(sessions!.Select(item => item.Id), Does.Not.Contain(scenario.OtherSession.Id));
            Assert.That(sessions!.Single(item => item.Id == scenario.Session.Id).CohortName,
                Is.EqualTo(scenario.Cohort.Name));
            Assert.That(sessions!.Single(item => item.Id == scenario.Session.Id).MentorName,
                Is.EqualTo(scenario.Mentor.DisplayName));
            Assert.That(sessions!.Single(item => item.Id == scenario.Session.Id).RosterCount,
                Is.EqualTo(2));
        });

        var crossCohort = await SendAsync(HttpMethod.Post, "/api/sessions", token, new
        {
            cohortId = scenario.OtherCohort.Id,
            mentorId = scenario.Mentor.Id,
            title = "Cross-cohort session",
            startsAt = DateTime.UtcNow.AddDays(1),
            repeatsWeekly = false,
        });
        Assert.That(crossCohort.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        var otherMentor = await SendAsync(HttpMethod.Post, "/api/sessions", token, new
        {
            cohortId = scenario.Cohort.Id,
            mentorId = scenario.OtherMentor.Id,
            title = "Wrong mentor session",
            startsAt = DateTime.UtcNow.AddDays(1),
            repeatsWeekly = false,
        });
        Assert.That(otherMentor.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        var recurrenceStart = DateTime.UtcNow.Date.AddDays(1).AddHours(18);
        var recurring = await SendAsync(HttpMethod.Post, "/api/sessions", token, new
        {
            cohortId = scenario.Cohort.Id,
            mentorId = scenario.Mentor.Id,
            title = "Mentor weekly lab",
            startsAt = recurrenceStart,
            durationMinutes = 75,
            location = "Studio 2",
            repeatsWeekly = true,
        });
        Assert.That(recurring.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var result = await recurring.Content.ReadFromJsonAsync<CreateSessionResult>();
        Assert.That(result!.GeneratedCount, Is.EqualTo(3));
    }

    [Test]
    [Property("Req", "L2-ENGAGE-002")]
    public async Task Attendance_roster_notes_history_scope_and_filtered_exports_are_complete()
    {
        // Traces to: L2-ENGAGE-002/003/004, L2-AUTH-010, L2-REPORT-003
        var scenario = await SeedScenarioAsync();
        var token = await SignInAsync(scenario.Mentor.Email);

        var rosterResponse = await SendAsync(
            HttpMethod.Get,
            $"/api/attendance/{scenario.Session.Id}/roster",
            token);
        var roster = await rosterResponse.Content.ReadFromJsonAsync<StaffSessionAttendance>();
        Assert.Multiple(() =>
        {
            Assert.That(roster!.CohortName, Is.EqualTo(scenario.Cohort.Name));
            Assert.That(roster.Roster.Select(item => item.YouthUserId),
                Is.EquivalentTo(new[] { scenario.YouthA.Id, scenario.YouthB.Id }));
            Assert.That(roster.Roster.All(item => item.Mark == null && item.Version == "none"), Is.True);
        });

        var initialHistory = await SendAsync(
            HttpMethod.Get,
            $"/api/attendance/history/by-session?cohortId={scenario.Cohort.Id}",
            token);
        var initialRows = await initialHistory.Content
            .ReadFromJsonAsync<List<SessionAttendanceSummary>>();
        Assert.That(
            initialRows!.Single(item => item.SessionId == scenario.Session.Id).Status,
            Is.EqualTo("needs review"));

        var save = await SendAsync(HttpMethod.Post, "/api/attendance", token, new
        {
            sessionId = scenario.Session.Id,
            marks = new object[]
            {
                new
                {
                    youthId = scenario.YouthA.Id,
                    status = "Present",
                    note = "Arrived ready to lead.",
                    expectedVersion = "none",
                },
                new
                {
                    youthId = scenario.YouthB.Id,
                    status = "Late",
                    note = (string?)null,
                    expectedVersion = "none",
                },
            },
        });
        var tally = await save.Content.ReadFromJsonAsync<AttendanceTally>();
        Assert.Multiple(() =>
        {
            Assert.That(save.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(tally!.Present, Is.EqualTo(1));
            Assert.That(tally.Late, Is.EqualTo(1));
            Assert.That(tally.Marked, Is.EqualTo(2));
        });

        var savedRoster = await SendAsync(
            HttpMethod.Get,
            $"/api/attendance/{scenario.Session.Id}/roster",
            token);
        var saved = await savedRoster.Content.ReadFromJsonAsync<StaffSessionAttendance>();
        Assert.Multiple(() =>
        {
            Assert.That(saved!.Roster.Single(item => item.YouthUserId == scenario.YouthA.Id).Note,
                Is.EqualTo("Arrived ready to lead."));
            Assert.That(saved.Roster.All(item => item.Version != "none"), Is.True);
        });

        var crossRoster = await SendAsync(
            HttpMethod.Get,
            $"/api/attendance/{scenario.OtherSession.Id}/roster",
            token);
        Assert.That(crossRoster.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        var crossSave = await SendAsync(HttpMethod.Post, "/api/attendance", token, new
        {
            sessionId = scenario.OtherSession.Id,
            marks = new[] { new { youthId = scenario.OtherYouth.Id, status = "Present" } },
        });
        Assert.That(crossSave.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        var from = DateTime.UtcNow.AddDays(-2);
        var to = DateTime.UtcNow.AddDays(2);
        var sessionHistory = await SendAsync(
            HttpMethod.Get,
            $"/api/attendance/history/by-session?cohortId={scenario.Cohort.Id}&from={from:O}&to={to:O}",
            token);
        var history = await sessionHistory.Content.ReadFromJsonAsync<List<SessionAttendanceSummary>>();
        var row = history!.Single(item => item.SessionId == scenario.Session.Id);
        Assert.Multiple(() =>
        {
            Assert.That(row.Present, Is.EqualTo(1));
            Assert.That(row.Marked, Is.EqualTo(2));
            Assert.That(row.RosterTotal, Is.EqualTo(2));
            Assert.That(row.Rate, Is.EqualTo(1.0));
            Assert.That(row.Status, Is.EqualTo("saved"));
        });

        var youthHistory = await SendAsync(
            HttpMethod.Get,
            $"/api/attendance/history/youth?cohortId={scenario.Cohort.Id}&from={from:O}&to={to:O}",
            token);
        var youthRows = await youthHistory.Content.ReadFromJsonAsync<List<YouthAttendanceSummary>>();
        Assert.Multiple(() =>
        {
            Assert.That(youthRows!.Select(item => item.YouthUserId),
                Is.EquivalentTo(new[] { scenario.YouthA.Id, scenario.YouthB.Id }));
            Assert.That(youthRows!.Single(item => item.YouthUserId == scenario.YouthB.Id).Late,
                Is.EqualTo(1));
            Assert.That(youthRows!.All(item => item.DisplayName.StartsWith("Calendar Youth")), Is.True);
        });

        var forbiddenYouth = await SendAsync(
            HttpMethod.Get,
            $"/api/attendance/history/by-youth?youthId={scenario.OtherYouth.Id}",
            token);
        Assert.That(forbiddenYouth.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        var csv = await SendAsync(
            HttpMethod.Get,
            $"/api/attendance/history/export?format=csv&view=session&cohortId={scenario.Cohort.Id}&from={from:O}&to={to:O}",
            token);
        var csvBody = await csv.Content.ReadAsStringAsync();
        Assert.Multiple(() =>
        {
            Assert.That(csv.Content.Headers.ContentType!.MediaType, Is.EqualTo("text/csv"));
            Assert.That(csvBody, Does.Contain(scenario.Session.Title));
            Assert.That(csvBody, Does.Contain(scenario.Cohort.Name));
            Assert.That(csvBody, Does.Not.Contain(scenario.OtherSession.Title));
        });
        var pdf = await SendAsync(
            HttpMethod.Get,
            $"/api/attendance/history/export?format=pdf&view=youth&cohortId={scenario.Cohort.Id}&from={from:O}&to={to:O}",
            token);
        var pdfBytes = await pdf.Content.ReadAsByteArrayAsync();
        Assert.Multiple(() =>
        {
            Assert.That(pdf.Content.Headers.ContentType!.MediaType, Is.EqualTo("application/pdf"));
            Assert.That(System.Text.Encoding.ASCII.GetString(pdfBytes, 0, 8), Is.EqualTo("%PDF-1.4"));
            Assert.That(pdfBytes.Length, Is.GreaterThan(500));
        });
        using var auditScope = Factory.Services.CreateScope();
        var auditDb = auditScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.That(
            auditDb.AuditEntries.Count(item =>
                item.Action == "DataExported"
                && item.TargetType == "Attendance"
                && item.TargetId == scenario.Cohort.Id
                && item.ActorUserId == scenario.Mentor.Id),
            Is.EqualTo(2));
    }

    private async Task<Scenario> SeedScenarioAsync()
    {
        var now = DateTime.UtcNow;
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var hash = hasher.Hash("Passw0rd!");
        var mentor = NewUser(Roles.Mentor, "Calendar Mentor", hash);
        var otherMentor = NewUser(Roles.Mentor, "Other Calendar Mentor", hash);
        var cohort = new Cohort
        {
            Name = $"Calendar Cohort {Guid.NewGuid():N}",
            MentorId = mentor.Id,
            TermStart = now.Date.AddDays(-30),
            TermEnd = now.Date.AddDays(15).AddHours(23),
        };
        var otherCohort = new Cohort
        {
            Name = $"Other Calendar Cohort {Guid.NewGuid():N}",
            MentorId = otherMentor.Id,
            TermStart = now.Date.AddDays(-30),
            TermEnd = now.Date.AddDays(30),
        };
        var youthA = NewUser(Roles.Youth, "Calendar Youth A", hash, mentor.Id, cohort.Id);
        var youthB = NewUser(Roles.Youth, "Calendar Youth B", hash, mentor.Id, cohort.Id);
        var otherYouth = NewUser(
            Roles.Youth,
            "Other Calendar Youth",
            hash,
            otherMentor.Id,
            otherCohort.Id);
        var session = new Session
        {
            CohortId = cohort.Id,
            MentorId = mentor.Id,
            Title = $"Leadership Circle {Guid.NewGuid():N}",
            StartsAt = now.AddHours(-1),
            DurationMinutes = 90,
            Location = "Studio 1",
            CreatedByUserId = mentor.Id,
        };
        var otherSession = new Session
        {
            CohortId = otherCohort.Id,
            MentorId = otherMentor.Id,
            Title = $"Private Other Circle {Guid.NewGuid():N}",
            StartsAt = now.AddHours(-1),
            CreatedByUserId = otherMentor.Id,
        };
        db.Users.AddRange(mentor, otherMentor, youthA, youthB, otherYouth);
        db.Cohorts.AddRange(cohort, otherCohort);
        db.Sessions.AddRange(session, otherSession);
        await db.SaveChangesAsync();
        return new Scenario(
            mentor,
            otherMentor,
            cohort,
            otherCohort,
            youthA,
            youthB,
            otherYouth,
            session,
            otherSession);
    }

    private static User NewUser(
        string role,
        string displayName,
        string passwordHash,
        Guid? mentorId = null,
        Guid? cohortId = null) => new()
        {
            Email = $"{displayName.Replace(" ", string.Empty).ToLowerInvariant()}.{Guid.NewGuid():N}@example.com",
            DisplayName = displayName,
            Role = role,
            Status = UserStatus.Active,
            PasswordHash = passwordHash,
            MentorId = mentorId,
            CohortId = cohortId,
        };

    private async Task<string> SignInAsync(string email)
    {
        var response = await Client.PostAsJsonAsync("/api/auth/sign-in", new
        {
            email,
            password = "Passw0rd!",
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResult>())!.AccessToken;
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string path,
        string token,
        object? body = null)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body is not null) request.Content = JsonContent.Create(body);
        Authorize(request, token);
        return await Client.SendAsync(request);
    }
}
