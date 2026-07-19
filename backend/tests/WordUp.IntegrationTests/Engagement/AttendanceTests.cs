// Acceptance Test
// Traces to: L2-ENGAGE-002 (attendance marking + tally; volunteer status restriction),
//            L2-ENGAGE-003 (idempotent re-save / offline sync: one record per session+youth)
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Engagement;

[TestFixture]
[Property("Req", "L2-ENGAGE-002")]
public sealed class AttendanceTests : IntegrationTestBase
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

    private async Task<(Guid CohortId, Guid SessionId, List<Guid> YouthIds)> SeedSessionWithYouthAsync(int youthCount)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var termStart = new DateTime(2026, 9, 1, 18, 0, 0, DateTimeKind.Utc);
        var cohort = new Cohort
        {
            Name = $"Cohort {Guid.NewGuid():N}",
            TermStart = termStart,
            TermEnd = termStart.AddDays(30),
        };
        db.Cohorts.Add(cohort);

        var session = new Session
        {
            CohortId = cohort.Id,
            Title = "Weekly Session",
            StartsAt = termStart,
            CreatedByUserId = Guid.NewGuid(),
        };
        db.Sessions.Add(session);

        var youthIds = new List<Guid>();
        for (var i = 0; i < youthCount; i++)
        {
            var youth = new User
            {
                Email = $"youth.{Guid.NewGuid():N}@example.com",
                DisplayName = $"Youth {i}",
                Role = Roles.Youth,
                Status = UserStatus.Active,
                CohortId = cohort.Id,
            };
            db.Users.Add(youth);
            youthIds.Add(youth.Id);
        }

        await db.SaveChangesAsync();
        return (cohort.Id, session.Id, youthIds);
    }

    [Test]
    [Property("Req", "L2-ENGAGE-002")]
    public async Task Marking_a_session_present_late_excused_absent_produces_the_expected_tally()
    {
        // Traces to: L2-ENGAGE-002
        var token = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var (_, sessionId, youth) = await SeedSessionWithYouthAsync(4);

        var response = await SendAsync(HttpMethod.Post, "/api/attendance", body: new
        {
            sessionId,
            marks = new[]
            {
                new { youthId = youth[0], status = "Present", note = (string?)null },
                new { youthId = youth[1], status = "Late", note = (string?)null },
                new { youthId = youth[2], status = "Excused", note = (string?)null },
                new { youthId = youth[3], status = "Absent", note = (string?)null },
            },
        }, token: token);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var tally = await response.Content.ReadFromJsonAsync<AttendanceTallyDto>();
        Assert.That(tally!.Present, Is.EqualTo(1));
        Assert.That(tally.Late, Is.EqualTo(1));
        Assert.That(tally.Excused, Is.EqualTo(1));
        Assert.That(tally.Absent, Is.EqualTo(1));
        Assert.That(tally.Marked, Is.EqualTo(4));
        Assert.That(tally.Total, Is.EqualTo(4));
    }

    [Test]
    [Property("Req", "L2-ENGAGE-002")]
    public async Task Bulk_marking_all_present_then_adjusting_one_to_absent_updates_the_tally()
    {
        // Traces to: L2-ENGAGE-002
        var token = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var (_, sessionId, youth) = await SeedSessionWithYouthAsync(3);

        var bulk = await SendAsync(HttpMethod.Post, "/api/attendance", body: new
        {
            sessionId,
            marks = youth.Select(y => new { youthId = y, status = "Present", note = (string?)null }).ToArray(),
        }, token: token);
        Assert.That(bulk.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var bulkTally = await bulk.Content.ReadFromJsonAsync<AttendanceTallyDto>();
        Assert.That(bulkTally!.Present, Is.EqualTo(3));

        var adjust = await SendAsync(HttpMethod.Post, "/api/attendance", body: new
        {
            sessionId,
            marks = new[] { new { youthId = youth[0], status = "Absent", note = (string?)null } },
        }, token: token);
        Assert.That(adjust.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var tally = await adjust.Content.ReadFromJsonAsync<AttendanceTallyDto>();
        Assert.That(tally!.Present, Is.EqualTo(2));
        Assert.That(tally.Absent, Is.EqualTo(1));
        Assert.That(tally.Marked, Is.EqualTo(3));
    }

    [Test]
    [Property("Req", "L2-ENGAGE-003")]
    public async Task Saving_the_same_marks_twice_is_idempotent_with_one_record_per_session_and_youth()
    {
        // Traces to: L2-ENGAGE-003
        var token = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var (_, sessionId, youth) = await SeedSessionWithYouthAsync(2);

        object body = new
        {
            sessionId,
            marks = new[]
            {
                new { youthId = youth[0], status = "Present", note = (string?)null },
                new { youthId = youth[1], status = "Absent", note = (string?)null },
            },
        };

        var first = await SendAsync(HttpMethod.Post, "/api/attendance", body: body, token: token);
        Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var second = await SendAsync(HttpMethod.Post, "/api/attendance", body: body, token: token);
        Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var count = db.AttendanceRecords.Count(r => r.SessionId == sessionId);
        Assert.That(count, Is.EqualTo(2)); // one row per (session, youth) despite two saves
    }

    [Test]
    [Property("Req", "L2-ENGAGE-002")]
    public async Task A_volunteer_marking_a_youth_late_is_forbidden()
    {
        // Traces to: L2-ENGAGE-002
        var token = await CreateUserAndGetTokenAsync(Roles.Volunteer);
        var (_, sessionId, youth) = await SeedSessionWithYouthAsync(1);

        var response = await SendAsync(HttpMethod.Post, "/api/attendance", body: new
        {
            sessionId,
            marks = new[] { new { youthId = youth[0], status = "Late", note = (string?)null } },
        }, token: token);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    [Property("Req", "L2-CURRIC-005")]
    [Property("Req", "L2-ENGAGE-002")]
    public async Task Five_consecutive_attendances_award_the_streak_badge_once_with_notifications()
    {
        // Traces to: L2-CURRIC-005 AC2; L2-ENGAGE-002 AC4
        var token = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        Guid youthId;
        Guid badgeId;
        List<Guid> sessionIds;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var startsAt = new DateTime(2026, 9, 1, 18, 0, 0, DateTimeKind.Utc);
            var cohort = new Cohort
            {
                Name = $"Streak cohort {Guid.NewGuid():N}",
                TermStart = startsAt,
                TermEnd = startsAt.AddDays(35),
            };
            var youth = new User
            {
                Email = $"streak.{Guid.NewGuid():N}@example.com",
                DisplayName = "Streak Youth",
                Role = Roles.Youth,
                Status = UserStatus.Active,
                CohortId = cohort.Id,
            };
            var sessions = Enumerable.Range(0, 5)
                .Select(index => new Session
                {
                    CohortId = cohort.Id,
                    Title = $"Streak session {index + 1}",
                    StartsAt = startsAt.AddDays(index * 7),
                    CreatedByUserId = Guid.NewGuid(),
                })
                .ToList();
            var badge = new Badge
            {
                Name = "Faithful Five",
                Icon = "zap",
                CriteriaKind = BadgeCriteriaKind.Auto,
                AutoRule = "attendance-streak",
            };
            db.Cohorts.Add(cohort);
            db.Users.Add(youth);
            db.Sessions.AddRange(sessions);
            db.Badges.Add(badge);
            await db.SaveChangesAsync();
            youthId = youth.Id;
            badgeId = badge.Id;
            sessionIds = sessions.Select(item => item.Id).ToList();
        }

        foreach (var sessionId in sessionIds)
        {
            var saved = await SendAsync(HttpMethod.Post, "/api/attendance", body: new
            {
                sessionId,
                marks = new[] { new { youthId, status = "Present", note = (string?)null } },
            }, token: token);
            Assert.That(saved.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
        var repeated = await SendAsync(HttpMethod.Post, "/api/attendance", body: new
        {
            sessionId = sessionIds[^1],
            marks = new[] { new { youthId, status = "Present", note = (string?)null } },
        }, token: token);
        Assert.That(repeated.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var verifyScope = Factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Multiple(() =>
        {
            Assert.That(verifyDb.BadgeAwards.Count(item =>
                item.YouthUserId == youthId && item.BadgeId == badgeId), Is.EqualTo(1));
            Assert.That(verifyDb.Notifications.Count(item =>
                item.UserId == youthId && item.EventKey == "badge-earned"), Is.EqualTo(1));
            Assert.That(Factory.Changes.Changes.Count(item =>
                item.Audience == "user"
                && item.AudienceId == youthId
                && item.EventName == "notification"
                && JsonSerializer.Serialize(item.Payload).Contains("badge-earned")), Is.EqualTo(1));
        });
    }

    private sealed record AttendanceTallyDto(int Present, int Late, int Excused, int Absent, int Marked, int Total);
}
