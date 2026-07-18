// Acceptance Test
// Traces to: L2-ENGAGE-001 (session scheduling: weekly recurrence materialised through term-end; cohort calendar)
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Engagement;

[TestFixture]
[Property("Req", "L2-ENGAGE-001")]
public sealed class SessionSchedulingTests : IntegrationTestBase
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

    private async Task<(Guid CohortId, Guid MentorId)> SeedCohortAsync(
        DateTime termStart,
        DateTime termEnd)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mentor = new User
        {
            Email = $"mentor.{Guid.NewGuid():N}@example.com",
            DisplayName = "Session Mentor",
            Role = Roles.Mentor,
            Status = UserStatus.Active,
        };
        var cohort = new Cohort
        {
            Name = $"Cohort {Guid.NewGuid():N}",
            TermStart = termStart,
            TermEnd = termEnd,
            MentorId = mentor.Id,
        };
        db.Users.Add(mentor);
        db.Cohorts.Add(cohort);
        await db.SaveChangesAsync();
        return (cohort.Id, mentor.Id);
    }

    [Test]
    [Property("Req", "L2-ENGAGE-001")]
    public async Task Weekly_recurring_session_generates_one_per_week_through_term_end_inclusive_and_never_beyond()
    {
        // Traces to: L2-ENGAGE-001
        var token = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var termStart = new DateTime(2026, 9, 1, 18, 0, 0, DateTimeKind.Utc);
        var termEnd = termStart.AddDays(28); // occurrences fall on days 0, 7, 14, 21, 28 => 5, inclusive
        var (cohortId, mentorId) = await SeedCohortAsync(termStart, termEnd);

        var response = await SendAsync(HttpMethod.Post, "/api/sessions", body: new
        {
            cohortId,
            mentorId,
            title = "Weekly Standup",
            startsAt = termStart,
            durationMinutes = 90,
            location = "Room A",
            repeatsWeekly = true,
        }, token: token);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var result = await response.Content.ReadFromJsonAsync<CreateSessionResultDto>();
        Assert.That(result!.GeneratedCount, Is.EqualTo(5));

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sessions = db.Sessions.Where(s => s.CohortId == cohortId).ToList();

        Assert.That(sessions.Count, Is.EqualTo(5));
        // No generated occurrence lands beyond the cohort's term-end.
        Assert.That(sessions.All(s => s.StartsAt <= termEnd), Is.True);
        // All occurrences share a single recurrence group.
        Assert.That(sessions.Select(s => s.RecurrenceGroupId).Distinct().Count(), Is.EqualTo(1));
        Assert.That(sessions.Single(s => s.Id == result.FirstSessionId).StartsAt, Is.EqualTo(termStart));
    }

    [Test]
    [Property("Req", "L2-ENGAGE-001")]
    public async Task Calendar_filtered_by_cohort_returns_only_that_cohorts_sessions()
    {
        // Traces to: L2-ENGAGE-001
        var token = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var termStart = new DateTime(2026, 10, 1, 18, 0, 0, DateTimeKind.Utc);
        var (cohortA, mentorA) = await SeedCohortAsync(termStart, termStart.AddDays(7));
        var (cohortB, mentorB) = await SeedCohortAsync(termStart, termStart.AddDays(7));

        await SendAsync(HttpMethod.Post, "/api/sessions", body: new
        {
            cohortId = cohortA,
            mentorId = mentorA,
            title = "Cohort A kickoff",
            startsAt = termStart,
            repeatsWeekly = false,
        }, token: token);
        await SendAsync(HttpMethod.Post, "/api/sessions", body: new
        {
            cohortId = cohortB,
            mentorId = mentorB,
            title = "Cohort B kickoff",
            startsAt = termStart,
            repeatsWeekly = false,
        }, token: token);

        var response = await SendAsync(HttpMethod.Get, $"/api/sessions?cohortId={cohortA}", token: token);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var sessions = await response.Content.ReadFromJsonAsync<List<SessionViewDto>>();
        Assert.That(sessions!.All(s => s.CohortId == cohortA), Is.True);
        Assert.That(sessions.Any(s => s.Title == "Cohort A kickoff"), Is.True);
        Assert.That(sessions.Any(s => s.Title == "Cohort B kickoff"), Is.False);
    }

    private sealed record CreateSessionResultDto(Guid FirstSessionId, int GeneratedCount);

    private sealed record SessionViewDto(
        Guid Id, Guid CohortId, Guid? MentorId, string Title, DateTime StartsAt, int? DurationMinutes, string? Location);
}
