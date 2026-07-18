// Acceptance Test
// Traces to: L2-NFR-006 (optimistic concurrency: stale save rejected with 409, version advances)
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.NonFunctional;

[TestFixture]
[Property("Req", "L2-NFR-006")]
public sealed class ConcurrencyTests : IntegrationTestBase
{
    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, object? body = null, string? token = null)
    {
        var request = new HttpRequestMessage(method, url);
        if (body is not null) request.Content = JsonContent.Create(body);
        if (token is not null) Authorize(request, token);
        return await Client.SendAsync(request);
    }

    private async Task<(Guid SessionId, Guid YouthId)> SeedSessionWithYouthAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var termStart = new DateTime(2026, 9, 1, 18, 0, 0, DateTimeKind.Utc);
        var cohort = new Cohort { Name = $"Cohort {Guid.NewGuid():N}", TermStart = termStart, TermEnd = termStart.AddDays(30) };
        db.Cohorts.Add(cohort);
        var session = new Session { CohortId = cohort.Id, Title = "Weekly Session", StartsAt = termStart, CreatedByUserId = Guid.NewGuid() };
        db.Sessions.Add(session);
        var youth = new User
        {
            Email = $"youth.{Guid.NewGuid():N}@example.com",
            DisplayName = "Youth",
            Role = Roles.Youth,
            Status = UserStatus.Active,
            CohortId = cohort.Id,
        };
        db.Users.Add(youth);
        await db.SaveChangesAsync();
        return (session.Id, youth.Id);
    }

    [Test]
    [Property("Req", "L2-NFR-006")]
    public async Task A_stale_attendance_save_is_rejected_with_conflict_and_the_version_advances()
    {
        // Traces to: L2-NFR-006 AC1/AC2
        var token = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var (sessionId, youthId) = await SeedSessionWithYouthAsync();

        // Establish the row (no expected version supplied → plain upsert).
        var first = await SendAsync(HttpMethod.Post, "/api/attendance", new
        {
            sessionId,
            marks = new[] { new { youthId, status = "Present", note = (string?)null } },
        }, token);
        Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Read the current version.
        var read = await SendAsync(HttpMethod.Get, $"/api/attendance/{sessionId}", token: token);
        var marks = await read.Content.ReadFromJsonAsync<List<MarkDto>>();
        var version = marks!.Single().Version;
        Assert.That(version, Is.Not.Null.And.Not.Empty);

        // Save against the version we read → succeeds and advances the version.
        var ok = await SendAsync(HttpMethod.Post, "/api/attendance", new
        {
            sessionId,
            marks = new[] { new { youthId, status = "Absent", note = (string?)null, expectedVersion = version } },
        }, token);
        Assert.That(ok.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var reread = await SendAsync(HttpMethod.Get, $"/api/attendance/{sessionId}", token: token);
        var newVersion = (await reread.Content.ReadFromJsonAsync<List<MarkDto>>())!.Single().Version;
        Assert.That(newVersion, Is.Not.EqualTo(version), "the version should advance after a successful save");

        // Re-using the now-stale version is rejected — no silent overwrite.
        var conflict = await SendAsync(HttpMethod.Post, "/api/attendance", new
        {
            sessionId,
            marks = new[] { new { youthId, status = "Present", note = (string?)null, expectedVersion = version } },
        }, token);
        Assert.That(conflict.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    private sealed record MarkDto(Guid YouthUserId, string Status, string? Note, DateTime RecordedAt, string? Version);
}
