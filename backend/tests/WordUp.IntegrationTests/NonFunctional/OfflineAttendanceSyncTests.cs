// Acceptance Test
// Traces to: L2-NFR-004 (offline attendance queue & sync integrity: a replayed queued write is applied
//            exactly once — retries never duplicate records)
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.NonFunctional;

[TestFixture]
[Property("Req", "L2-NFR-004")]
public sealed class OfflineAttendanceSyncTests : IntegrationTestBase
{
    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string url,
        object? body = null,
        string? token = null,
        string? idempotencyKey = null)
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
        if (idempotencyKey is not null)
        {
            request.Headers.Add("Idempotency-Key", idempotencyKey);
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
    [Property("Req", "L2-NFR-004")]
    public async Task Replaying_identical_offline_attendance_marks_yields_exactly_one_record_per_session_and_youth()
    {
        // Traces to: L2-NFR-004
        var token = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var (_, sessionId, youth) = await SeedSessionWithYouthAsync(3);

        // A batch queued while offline, then replayed on reconnect (a retry) must be applied exactly once (AC2).
        object body = new
        {
            sessionId,
            marks = new[]
            {
                new { youthId = youth[0], status = "Present", note = (string?)null },
                new { youthId = youth[1], status = "Late", note = (string?)null },
                new { youthId = youth[2], status = "Absent", note = (string?)null },
            },
        };

        var idempotencyKey = Guid.NewGuid().ToString();
        var first = await SendAsync(
            HttpMethod.Post,
            "/api/attendance",
            body: body,
            token: token,
            idempotencyKey: idempotencyKey);
        Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        Guid firstVersion;
        using (var firstScope = Factory.Services.CreateScope())
        {
            var firstDb = firstScope.ServiceProvider.GetRequiredService<AppDbContext>();
            firstVersion = firstDb.AttendanceRecords.Single(r =>
                r.SessionId == sessionId && r.YouthUserId == youth[0]).ConcurrencyStamp;
        }

        var replay = await SendAsync(
            HttpMethod.Post,
            "/api/attendance",
            body: body,
            token: token,
            idempotencyKey: idempotencyKey);
        Assert.That(replay.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var mismatchedReplay = await SendAsync(
            HttpMethod.Post,
            "/api/attendance",
            body: new
            {
                sessionId,
                marks = new[]
                {
                    new { youthId = youth[0], status = "Absent", note = (string?)null },
                },
            },
            token: token,
            idempotencyKey: idempotencyKey);
        Assert.That(mismatchedReplay.StatusCode, Is.EqualTo(HttpStatusCode.Conflict),
            "an idempotency key cannot be reused for a different operation");

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var total = db.AttendanceRecords.Count(r => r.SessionId == sessionId);
        Assert.That(total, Is.EqualTo(3), "exactly one record per (session, youth) despite the replay");
        Assert.That(
            db.AttendanceRecords.Single(r => r.SessionId == sessionId && r.YouthUserId == youth[0]).ConcurrencyStamp,
            Is.EqualTo(firstVersion),
            "replaying an idempotency key must not execute the write a second time");
        Assert.That(
            db.AuditEntries.Count(a => a.Action == "AttendanceSaved" && a.TargetId == sessionId),
            Is.EqualTo(1),
            "exactly-once replay must not duplicate the audit side effect");

        foreach (var y in youth)
        {
            var perYouth = db.AttendanceRecords.Count(r => r.SessionId == sessionId && r.YouthUserId == y);
            Assert.That(perYouth, Is.EqualTo(1), "each youth has a single upserted record, not a duplicate");
        }
    }
}
