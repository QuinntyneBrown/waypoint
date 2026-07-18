// Acceptance Test
// Traces to: L2-ENGAGE-004 (attendance history: per-session turnout rate/status; per-youth rate + below-threshold flag)
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Engagement;

[TestFixture]
[Property("Req", "L2-ENGAGE-004")]
public sealed class AttendanceHistoryTests : IntegrationTestBase
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

    private async Task<(Guid CohortId, List<Guid> SessionIds, List<Guid> YouthIds)> SeedCohortSessionsYouthAsync(int sessionCount, int youthCount)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var termStart = new DateTime(2026, 9, 1, 18, 0, 0, DateTimeKind.Utc);
        var cohort = new Cohort
        {
            Name = $"Cohort {Guid.NewGuid():N}",
            TermStart = termStart,
            TermEnd = termStart.AddDays(90),
        };
        db.Cohorts.Add(cohort);

        var sessionIds = new List<Guid>();
        for (var i = 0; i < sessionCount; i++)
        {
            var session = new Session
            {
                CohortId = cohort.Id,
                Title = $"Session {i}",
                StartsAt = termStart.AddDays(i * 7),
                CreatedByUserId = Guid.NewGuid(),
            };
            db.Sessions.Add(session);
            sessionIds.Add(session.Id);
        }

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
        return (cohort.Id, sessionIds, youthIds);
    }

    [Test]
    [Property("Req", "L2-ENGAGE-004")]
    public async Task By_session_history_reports_present_count_rate_and_turnout_status()
    {
        // Traces to: L2-ENGAGE-004
        var token = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var (cohortId, sessions, youth) = await SeedCohortSessionsYouthAsync(sessionCount: 2, youthCount: 4);

        // High-turnout session: 3 present, 1 absent => rate 0.75 => "saved".
        await SendAsync(HttpMethod.Post, "/api/attendance", body: new
        {
            sessionId = sessions[0],
            marks = new[]
            {
                new { youthId = youth[0], status = "Present", note = (string?)null },
                new { youthId = youth[1], status = "Present", note = (string?)null },
                new { youthId = youth[2], status = "Present", note = (string?)null },
                new { youthId = youth[3], status = "Absent", note = (string?)null },
            },
        }, token: token);

        // Low-turnout session: 1 present, 3 absent => rate 0.25 => "low turnout".
        await SendAsync(HttpMethod.Post, "/api/attendance", body: new
        {
            sessionId = sessions[1],
            marks = new[]
            {
                new { youthId = youth[0], status = "Present", note = (string?)null },
                new { youthId = youth[1], status = "Absent", note = (string?)null },
                new { youthId = youth[2], status = "Absent", note = (string?)null },
                new { youthId = youth[3], status = "Absent", note = (string?)null },
            },
        }, token: token);

        var response = await SendAsync(HttpMethod.Get, $"/api/attendance/history/by-session?cohortId={cohortId}", token: token);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var summaries = await response.Content.ReadFromJsonAsync<List<SessionAttendanceSummaryDto>>();

        var high = summaries!.Single(s => s.SessionId == sessions[0]);
        Assert.That(high.Present, Is.EqualTo(3));
        Assert.That(high.Marked, Is.EqualTo(4));
        Assert.That(high.Rate, Is.EqualTo(0.75));
        Assert.That(high.Status, Is.EqualTo("saved"));

        var low = summaries!.Single(s => s.SessionId == sessions[1]);
        Assert.That(low.Present, Is.EqualTo(1));
        Assert.That(low.Marked, Is.EqualTo(4));
        Assert.That(low.Rate, Is.EqualTo(0.25));
        Assert.That(low.Status, Is.EqualTo("low turnout"));
    }

    [Test]
    [Property("Req", "L2-ENGAGE-004")]
    public async Task By_youth_history_reports_counts_rate_and_below_threshold_flag()
    {
        // Traces to: L2-ENGAGE-004
        var token = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var (_, sessions, youth) = await SeedCohortSessionsYouthAsync(sessionCount: 3, youthCount: 2);
        var youthLow = youth[0];
        var youthHigh = youth[1];

        // youthLow attends 1 of 3 (rate 0.33 -> below threshold); youthHigh attends all 3.
        var lowStatuses = new[] { "Present", "Absent", "Absent" };
        for (var i = 0; i < sessions.Count; i++)
        {
            await SendAsync(HttpMethod.Post, "/api/attendance", body: new
            {
                sessionId = sessions[i],
                marks = new[]
                {
                    new { youthId = youthLow, status = lowStatuses[i], note = (string?)null },
                    new { youthId = youthHigh, status = "Present", note = (string?)null },
                },
            }, token: token);
        }

        var lowResponse = await SendAsync(HttpMethod.Get, $"/api/attendance/history/by-youth?youthId={youthLow}", token: token);
        Assert.That(lowResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var low = await lowResponse.Content.ReadFromJsonAsync<YouthAttendanceSummaryDto>();
        Assert.That(low!.Attended, Is.EqualTo(1));
        Assert.That(low.Total, Is.EqualTo(3));
        Assert.That(low.Rate, Is.EqualTo(1.0 / 3).Within(0.001));
        Assert.That(low.BelowThreshold, Is.True);
        Assert.That(low.Trend, Is.EqualTo("declining"));

        var highResponse = await SendAsync(HttpMethod.Get, $"/api/attendance/history/by-youth?youthId={youthHigh}", token: token);
        Assert.That(highResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var high = await highResponse.Content.ReadFromJsonAsync<YouthAttendanceSummaryDto>();
        Assert.That(high!.Attended, Is.EqualTo(3));
        Assert.That(high.Total, Is.EqualTo(3));
        Assert.That(high.Rate, Is.EqualTo(1.0));
        Assert.That(high.BelowThreshold, Is.False);
        Assert.That(high.Trend, Is.EqualTo("steady"));
    }

    private sealed record SessionAttendanceSummaryDto(
        Guid SessionId, string Title, DateTime StartsAt, int Present, int Marked, double Rate, string Status);

    private sealed record YouthAttendanceSummaryDto(
        Guid YouthUserId,
        int Attended,
        int Late,
        int Excused,
        int Absent,
        int Total,
        double Rate,
        bool BelowThreshold,
        string DisplayName,
        string Trend);
}
