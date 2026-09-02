// Acceptance Test
// Traces to: L2-REPORT-001 (youth & cohort reports, staff-view scoping, empty-state)
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Authentication.Auth;
using Waypoint.Infrastructure;

namespace Waypoint.IntegrationTests.Reporting;

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

    private async Task<string> SignInAsync(string email, string password = "Passw0rd!")
    {
        var response = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password });
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResult>();
        return auth!.AccessToken;
    }

    private async Task<SeededReportFixture> SeedReportFixtureAsync()
    {
        var response = await Client.PostAsync("/api/_test/youth-curriculum", null);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SeededReportFixture>())!;
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

    [Test]
    [Property("Req", "L2-REPORT-001")]
    public async Task Filtered_hub_and_detail_reports_return_complete_authoritative_views()
    {
        // Traces to: L2-REPORT-001 AC1..5
        var seeded = await SeedReportFixtureAsync();
        var comparison = await SeedReportFixtureAsync();
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var previousCohort = await db.Cohorts.FindAsync(comparison.CohortId);
            previousCohort!.TermStart = DateTime.UtcNow.AddYears(-2);
            previousCohort.TermEnd = DateTime.UtcNow.AddYears(-1);
            await db.SaveChangesAsync();
        }
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var from = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
        var to = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");
        var filter = $"from={from}&to={to}&cohortId={seeded.CohortId}&trackId={seeded.PrimaryTrackId}&mentorId={seeded.MentorId}";

        var optionsResponse = await SendAsync(HttpMethod.Get, "/api/reports/options", token: adminToken);
        Assert.That(optionsResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var options = await optionsResponse.Content.ReadFromJsonAsync<ReportOptionsDto>();
        Assert.Multiple(() =>
        {
            Assert.That(options!.Cohorts.Any(item => item.Id == seeded.CohortId && item.Name == seeded.CohortName), Is.True);
            Assert.That(options.Tracks.Any(item => item.Id == seeded.PrimaryTrackId && item.Name == seeded.PrimaryTrackName), Is.True);
            Assert.That(options.Mentors.Any(item => item.Id == seeded.MentorId && item.Name == seeded.MentorName), Is.True);
            Assert.That(options.Youth.Any(item => item.Id == seeded.YouthId && item.Name == seeded.YouthName), Is.True);
        });

        var hubResponse = await SendAsync(HttpMethod.Get, $"/api/reports/hub?{filter}", token: adminToken);
        Assert.That(hubResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var hub = await hubResponse.Content.ReadFromJsonAsync<ReportsHubDto>();
        Assert.Multiple(() =>
        {
            Assert.That(hub!.GeneratedAt, Is.Not.EqualTo(default(DateTime)));
            Assert.That(hub.RecordCount, Is.EqualTo(1));
            Assert.That(hub.Kpis.ActiveYouth, Is.EqualTo(1));
            Assert.That(hub.Kpis.AvgCompletion, Is.EqualTo(0.5d));
            Assert.That(hub.Attendance.Present, Is.EqualTo(1));
            Assert.That(hub.Attendance.Absent, Is.EqualTo(1));
            Assert.That(hub.Attendance.Total, Is.EqualTo(2));
            Assert.That(hub.Cohorts.Single().CohortName, Is.EqualTo(seeded.CohortName));
            Assert.That(hub.Staff.Single(item => item.MentorId == seeded.MentorId).CheckIns, Is.EqualTo(1));
        });

        var recentHubResponse = await SendAsync(
            HttpMethod.Get,
            $"/api/reports/hub?from={DateTime.UtcNow.AddDays(-5):yyyy-MM-dd}&to={to}&cohortId={seeded.CohortId}",
            token: adminToken);
        var recentHub = await recentHubResponse.Content.ReadFromJsonAsync<ReportsHubDto>();
        Assert.That(recentHub!.Attendance.Total, Is.Zero, "date filters must recompute activity metrics");

        var youthResponse = await SendAsync(
            HttpMethod.Get,
            $"/api/reports/youth/{seeded.YouthId}?from={from}&to={to}&trackId={seeded.PrimaryTrackId}",
            token: adminToken);
        var youth = await youthResponse.Content.ReadFromJsonAsync<CompleteYouthReportDto>();
        Assert.Multiple(() =>
        {
            Assert.That(youth!.YouthName, Is.EqualTo(seeded.YouthName));
            Assert.That(youth.CompletionRate, Is.EqualTo(0.5d));
            Assert.That(youth.Modules.Single(item => item.Name == seeded.FirstModuleName).CompletionRate, Is.EqualTo(0.5d));
            Assert.That(youth.QuizScores.Any(item => item.Title == seeded.ProgressQuizTitle && item.Score == 82), Is.True);
            Assert.That(youth.Attendance.Present, Is.EqualTo(1));
            Assert.That(youth.Attendance.Absent, Is.EqualTo(1));
            Assert.That(youth.Competencies.Any(item => item.Name == seeded.ProgressCompetencyName && item.Level == "Proficient"), Is.True);
            Assert.That(youth.MentorNotes.CheckIns, Is.EqualTo(1));
            Assert.That(youth.BadgeAwards.Any(item => item.Name == seeded.JourneyBadgeName), Is.True);
        });

        var cohortResponse = await SendAsync(
            HttpMethod.Get,
            $"/api/reports/cohort/{seeded.CohortId}?from={from}&to={to}&trackId={seeded.PrimaryTrackId}&compareCohortId={comparison.CohortId}",
            token: adminToken);
        var cohort = await cohortResponse.Content.ReadFromJsonAsync<CompleteCohortReportDto>();
        Assert.Multiple(() =>
        {
            Assert.That(cohort!.CohortName, Is.EqualTo(seeded.CohortName));
            Assert.That(cohort.Enrolment, Is.EqualTo(1));
            Assert.That(cohort.RetentionRate, Is.EqualTo(1d));
            Assert.That(cohort.Modules.Single(item => item.Name == seeded.FirstModuleName).CompletionRate, Is.EqualTo(0.5d));
            Assert.That(cohort.Youth.Single().Status, Is.EqualTo("At risk"));
            Assert.That(cohort.Comparisons.Any(item => item.Label == "Program average"), Is.True);
            Assert.That(cohort.Comparisons.Any(item => item.Label.StartsWith("Compared cohort:")), Is.True);
            Assert.That(cohort.Comparisons.Any(item => item.Label == "Previous term"), Is.True);
        });

        var staffResponse = await SendAsync(HttpMethod.Get, $"/api/reports/staff-activity?{filter}", token: adminToken);
        var staff = await staffResponse.Content.ReadFromJsonAsync<List<StaffActivityDto>>();
        Assert.Multiple(() =>
        {
            Assert.That(staff, Has.Count.EqualTo(1));
            Assert.That(staff![0].MentorName, Is.EqualTo(seeded.MentorName));
            Assert.That(staff[0].Caseload, Is.EqualTo(1));
            Assert.That(staff[0].CheckIns, Is.EqualTo(1));
            Assert.That(staff[0].LoadBalance, Is.EqualTo("Under target"));
        });
    }

    [Test]
    [Property("Req", "L2-REPORT-001")]
    public async Task Mentor_report_scope_contains_only_their_youth_and_cohorts()
    {
        // Traces to: L2-REPORT-001 AC6, L2-AUTH-010
        var mine = await SeedReportFixtureAsync();
        var other = await SeedReportFixtureAsync();
        var mentorToken = await SignInAsync(mine.MentorEmail, mine.MentorPassword);

        var optionsResponse = await SendAsync(HttpMethod.Get, "/api/reports/options", token: mentorToken);
        var options = await optionsResponse.Content.ReadFromJsonAsync<ReportOptionsDto>();
        Assert.Multiple(() =>
        {
            Assert.That(options!.Youth.Select(item => item.Id), Does.Contain(mine.YouthId));
            Assert.That(options.Youth.Select(item => item.Id), Does.Not.Contain(other.YouthId));
            Assert.That(options.Cohorts.Select(item => item.Id), Does.Contain(mine.CohortId));
            Assert.That(options.Cohorts.Select(item => item.Id), Does.Not.Contain(other.CohortId));
        });

        var hubResponse = await SendAsync(HttpMethod.Get, "/api/reports/hub", token: mentorToken);
        var hub = await hubResponse.Content.ReadFromJsonAsync<ReportsHubDto>();
        Assert.That(hub!.RecordCount, Is.EqualTo(1));

        var ownCohort = await SendAsync(HttpMethod.Get, $"/api/reports/cohort/{mine.CohortId}", token: mentorToken);
        Assert.That(ownCohort.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var denied = await SendAsync(HttpMethod.Get, $"/api/reports/cohort/{other.CohortId}", token: mentorToken);
        Assert.That(denied.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    private sealed record YouthReportDto(Guid YouthId, int LessonsCompleted, double AvgQuizScore, int AttendancePresent, int AttendanceTotal, int Badges);
    private sealed record CohortReportDto(Guid CohortId, int Enrolment, double AvgCompletion, double AttendanceRate, int StatusActive, int StatusAtRisk);

    private sealed record SeededReportFixture(
        Guid YouthId,
        string YouthName,
        Guid CohortId,
        string CohortName,
        Guid PrimaryTrackId,
        string PrimaryTrackName,
        string FirstModuleName,
        Guid MentorId,
        string MentorName,
        string MentorEmail,
        string MentorPassword,
        string ProgressQuizTitle,
        string ProgressCompetencyName,
        string JourneyBadgeName);

    private sealed record ReportOptionDto(Guid Id, string Name);
    private sealed record ReportOptionsDto(
        IReadOnlyList<ReportOptionDto> Cohorts,
        IReadOnlyList<ReportOptionDto> Tracks,
        IReadOnlyList<ReportOptionDto> Mentors,
        IReadOnlyList<ReportOptionDto> Youth);
    private sealed record AttendanceBreakdownDto(int Present, int Late, int Excused, int Absent, int Total, double Rate);
    private sealed record ReportKpisDto(int ActiveYouth, int Cohorts, double AvgCompletion, double AttendanceRate, double RetentionRate, int ActiveMentors);
    private sealed record CohortMetricDto(Guid CohortId, string CohortName, int Enrolment, double CompletionRate, double AttendanceRate, double RetentionRate, int AtRisk);
    private sealed record StaffActivityDto(Guid MentorId, string MentorName, int Caseload, int CheckIns, int Reviews, string LoadBalance);
    private sealed record ReportsHubDto(
        DateTime GeneratedAt,
        int RecordCount,
        ReportKpisDto Kpis,
        AttendanceBreakdownDto Attendance,
        IReadOnlyList<CohortMetricDto> Cohorts,
        IReadOnlyList<StaffActivityDto> Staff);
    private sealed record ModuleReportDto(Guid ModuleId, string Name, int CompletedLessons, int TotalLessons, double CompletionRate);
    private sealed record QuizScoreDto(string Title, int Score);
    private sealed record CompetencyReportDto(string Name, string Level);
    private sealed record MentorNotesDto(int CheckIns, DateTime? LatestAt, string? LatestEngagement);
    private sealed record BadgeReportDto(string Name, DateTime AwardedAt);
    private sealed record CompleteYouthReportDto(
        Guid YouthId,
        string YouthName,
        double CompletionRate,
        AttendanceBreakdownDto Attendance,
        IReadOnlyList<ModuleReportDto> Modules,
        IReadOnlyList<QuizScoreDto> QuizScores,
        IReadOnlyList<CompetencyReportDto> Competencies,
        MentorNotesDto MentorNotes,
        IReadOnlyList<BadgeReportDto> BadgeAwards);
    private sealed record YouthSummaryDto(Guid YouthId, string YouthName, string Status);
    private sealed record ComparisonDto(string Label, double CompletionRate, double AttendanceRate, double RetentionRate);
    private sealed record CompleteCohortReportDto(
        Guid CohortId,
        string CohortName,
        int Enrolment,
        double RetentionRate,
        IReadOnlyList<ModuleReportDto> Modules,
        IReadOnlyList<YouthSummaryDto> Youth,
        IReadOnlyList<ComparisonDto> Comparisons);
}
