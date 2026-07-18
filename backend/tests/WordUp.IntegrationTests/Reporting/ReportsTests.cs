// Acceptance Test
// Traces to: L2-REPORT-001 (youth & cohort reports, staff-view scoping, empty-state)
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Abstractions;
using WordUp.Application.Auth;
using WordUp.Domain;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Reporting;

[TestFixture]
[Property("Req", "L2-REPORT-001")]
public sealed class ReportsTests : IntegrationTestBase
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

    private async Task<(Guid Id, string Email)> SeedUserAsync(string role, Guid? mentorId = null, Guid? cohortId = null)
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
            MentorId = mentorId,
            CohortId = cohortId,
            PasswordHash = hasher.Hash("Passw0rd!"),
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return (user.Id, email);
    }

    private async Task<string> SignInAsync(string email)
    {
        var response = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password = "Passw0rd!" });
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResult>();
        return auth!.AccessToken;
    }

    [Test]
    [Property("Req", "L2-REPORT-001")]
    public async Task Admin_gets_a_youth_report_with_correct_aggregates()
    {
        // Traces to: L2-REPORT-001
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var mentor = await SeedUserAsync(Roles.Mentor);
        var youth = await SeedUserAsync(Roles.Youth, mentorId: mentor.Id);

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.LessonProgress.Add(new LessonProgress { YouthUserId = youth.Id, LessonId = Guid.NewGuid(), Status = ProgressStatus.Completed, CompletedAt = DateTime.UtcNow });
            db.LessonProgress.Add(new LessonProgress { YouthUserId = youth.Id, LessonId = Guid.NewGuid(), Status = ProgressStatus.Completed, CompletedAt = DateTime.UtcNow });
            db.LessonProgress.Add(new LessonProgress { YouthUserId = youth.Id, LessonId = Guid.NewGuid(), Status = ProgressStatus.InProgress });

            // AvgQuizScore uses ApprovedScore when present, else AutoScore: (90, 70) => 80.
            db.QuizAttempts.Add(new QuizAttempt { YouthUserId = youth.Id, QuizId = Guid.NewGuid(), AutoScore = 80, ApprovedScore = 90, Passed = true, IsSubmitted = true });
            db.QuizAttempts.Add(new QuizAttempt { YouthUserId = youth.Id, QuizId = Guid.NewGuid(), AutoScore = 70, Passed = true, IsSubmitted = true });

            db.AttendanceRecords.Add(new AttendanceRecord { SessionId = Guid.NewGuid(), YouthUserId = youth.Id, Status = AttendanceStatus.Present, RecordedByUserId = mentor.Id });
            db.AttendanceRecords.Add(new AttendanceRecord { SessionId = Guid.NewGuid(), YouthUserId = youth.Id, Status = AttendanceStatus.Late, RecordedByUserId = mentor.Id });
            db.AttendanceRecords.Add(new AttendanceRecord { SessionId = Guid.NewGuid(), YouthUserId = youth.Id, Status = AttendanceStatus.Absent, RecordedByUserId = mentor.Id });

            db.BadgeAwards.Add(new BadgeAward { BadgeId = Guid.NewGuid(), YouthUserId = youth.Id });
            await db.SaveChangesAsync();
        }

        var response = await SendAsync(HttpMethod.Get, $"/api/reports/youth/{youth.Id}", token: adminToken);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var report = await response.Content.ReadFromJsonAsync<YouthReportDto>();
        Assert.That(report!.LessonsCompleted, Is.EqualTo(2));
        Assert.That(report.AvgQuizScore, Is.EqualTo(80d));
        Assert.That(report.AttendancePresent, Is.EqualTo(2)); // Present + Late count as present
        Assert.That(report.AttendanceTotal, Is.EqualTo(3));
        Assert.That(report.Badges, Is.EqualTo(1));
    }

    [Test]
    [Property("Req", "L2-REPORT-001")]
    public async Task A_mentor_can_view_an_assigned_youth_but_is_forbidden_from_another_mentors_youth()
    {
        // Traces to: L2-REPORT-001 (AC6 scoping / L2-AUTH-010 no-IDOR)
        var mentor = await SeedUserAsync(Roles.Mentor);
        var otherMentor = await SeedUserAsync(Roles.Mentor);
        var mine = await SeedUserAsync(Roles.Youth, mentorId: mentor.Id);
        var notMine = await SeedUserAsync(Roles.Youth, mentorId: otherMentor.Id);
        var mentorToken = await SignInAsync(mentor.Email);

        var allowed = await SendAsync(HttpMethod.Get, $"/api/reports/youth/{mine.Id}", token: mentorToken);
        Assert.That(allowed.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var forbidden = await SendAsync(HttpMethod.Get, $"/api/reports/youth/{notMine.Id}", token: mentorToken);
        Assert.That(forbidden.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    [Property("Req", "L2-REPORT-001")]
    public async Task A_cohort_with_no_youth_returns_zeros_as_an_empty_state()
    {
        // Traces to: L2-REPORT-001 (empty-state returns zeros, not an error)
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var emptyCohortId = Guid.NewGuid();

        var response = await SendAsync(HttpMethod.Get, $"/api/reports/cohort/{emptyCohortId}", token: adminToken);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var report = await response.Content.ReadFromJsonAsync<CohortReportDto>();
        Assert.That(report!.Enrolment, Is.EqualTo(0));
        Assert.That(report.AvgCompletion, Is.EqualTo(0d));
        Assert.That(report.AttendanceRate, Is.EqualTo(0d));
        Assert.That(report.StatusActive, Is.EqualTo(0));
        Assert.That(report.StatusAtRisk, Is.EqualTo(0));
    }

    private sealed record YouthReportDto(Guid YouthId, int LessonsCompleted, double AvgQuizScore, int AttendancePresent, int AttendanceTotal, int Badges);
    private sealed record CohortReportDto(Guid CohortId, int Enrolment, double AvgCompletion, double AttendanceRate, int StatusActive, int StatusAtRisk);
}
