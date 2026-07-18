using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Authentication.Auth;
using WordUp.Application.Mentoring.MentorRoster;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Mentoring;

// Acceptance Test
// Traces to: L2-MENTOR-001 (dashboard/roster), L2-MENTOR-002 (youth detail), L2-AUTH-010 (mentor scoping)
[TestFixture]
[Property("Req", "L2-MENTOR-001")]
public sealed class RosterTests : IntegrationTestBase
{
    private sealed record Scenario(Guid MentorId, string MentorEmail, Guid YouthA, Guid YouthB, Guid OtherYouth);
    private sealed record DashboardAcceptance(
        int YouthCount,
        int PendingReviews,
        int OverdueReviews,
        double AverageProgress,
        int AtRiskCount);
    private sealed record ReviewAcceptance(
        Guid ReviewId,
        Guid YouthId,
        string YouthName,
        string ItemType,
        string ItemTitle,
        DateTime SubmittedAt,
        DateTime? DueDate,
        bool Overdue,
        string ReviewPath);
    private sealed record RosterAcceptance(
        Guid Id,
        string DisplayName,
        string Status,
        bool AtRisk,
        bool New,
        double Progress,
        Guid? CohortId,
        string? CohortName,
        DateTime LastActiveAt);

    private async Task<Scenario> SeedMentorWithYouthAsync()
    {
        var mentorId = Guid.NewGuid();
        var otherMentorId = Guid.NewGuid();
        var youthA = Guid.NewGuid();
        var youthB = Guid.NewGuid();
        var otherYouth = Guid.NewGuid();
        var mentorEmail = $"mentor.{Guid.NewGuid():N}@example.com";

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var hash = hasher.Hash("Passw0rd!");

        db.Users.AddRange(
            new User { Id = mentorId, Email = mentorEmail, DisplayName = "Mentor One", Role = Roles.Mentor, Status = UserStatus.Active, PasswordHash = hash },
            new User { Id = otherMentorId, Email = $"mentor2.{Guid.NewGuid():N}@example.com", DisplayName = "Mentor Two", Role = Roles.Mentor, Status = UserStatus.Active, PasswordHash = hash },
            new User { Id = youthA, Email = $"youtha.{Guid.NewGuid():N}@example.com", DisplayName = "Aaron Youth", Role = Roles.Youth, Status = UserStatus.Active, MentorId = mentorId, PasswordHash = hash },
            new User { Id = youthB, Email = $"youthb.{Guid.NewGuid():N}@example.com", DisplayName = "Bianca Youth", Role = Roles.Youth, Status = UserStatus.Active, MentorId = mentorId, PasswordHash = hash },
            new User { Id = otherYouth, Email = $"youthc.{Guid.NewGuid():N}@example.com", DisplayName = "Carl Youth", Role = Roles.Youth, Status = UserStatus.Active, MentorId = otherMentorId, PasswordHash = hash });

        await db.SaveChangesAsync();

        return new Scenario(mentorId, mentorEmail, youthA, youthB, otherYouth);
    }

    private async Task<string> SignInAsync(string email)
    {
        var response = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password = "Passw0rd!" });
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResult>();
        return auth!.AccessToken;
    }

    private async Task<HttpResponseMessage> GetAsync(string url, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    [Test]
    [Property("Req", "L2-MENTOR-001")]
    public async Task Dashboard_and_roster_count_only_the_mentors_assigned_youth()
    {
        // Traces to: L2-MENTOR-001, L2-AUTH-010
        var scenario = await SeedMentorWithYouthAsync();
        var token = await SignInAsync(scenario.MentorEmail);

        var dashboardResponse = await GetAsync("/api/mentor/dashboard", token);
        Assert.That(dashboardResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var dashboard = await dashboardResponse.Content.ReadFromJsonAsync<MentorDashboard>();
        Assert.That(dashboard!.YouthCount, Is.EqualTo(2));

        var rosterResponse = await GetAsync("/api/mentor/youth", token);
        Assert.That(rosterResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var roster = await rosterResponse.Content.ReadFromJsonAsync<List<RosterYouth>>();

        Assert.That(roster!.Count, Is.EqualTo(2));
        var ids = roster.Select(r => r.Id).ToHashSet();
        Assert.That(ids, Does.Contain(scenario.YouthA));
        Assert.That(ids, Does.Contain(scenario.YouthB));
        Assert.That(ids, Does.Not.Contain(scenario.OtherYouth));
    }

    [Test]
    [Property("Req", "L2-MENTOR-001")]
    [Property("Req", "L2-AUTH-010")]
    public async Task Dashboard_filters_and_review_preview_are_complete_and_strictly_scoped()
    {
        // Traces to: L2-MENTOR-001 AC1-AC4, L2-AUTH-010
        var scenario = await SeedMentorWithYouthAsync();
        var now = DateTime.UtcNow;
        Guid overdueSubmissionId;
        Guid pendingQuizAttemptId;

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var youthA = await db.Users.FindAsync(scenario.YouthA);
            var youthB = await db.Users.FindAsync(scenario.YouthB);
            youthA!.CreatedAt = now.AddDays(-45);
            youthA.LastSignInAt = now.AddDays(-3);
            youthB!.CreatedAt = now.AddDays(-5);
            youthB.LastSignInAt = now.AddHours(-2);

            var overdueAssignment = new Assignment
            {
                Title = "Overdue reflection",
                TaskDescription = "Reflect on this week's lesson.",
                SubmissionTypes = "text",
                DueDate = now.AddDays(-1),
            };
            var otherAssignment = new Assignment
            {
                Title = "Another mentor's overdue work",
                TaskDescription = "Must remain private.",
                SubmissionTypes = "text",
                DueDate = now.AddDays(-2),
            };
            db.Assignments.AddRange(overdueAssignment, otherAssignment);

            var overdueSubmission = new Submission
            {
                YouthUserId = scenario.YouthA,
                AssignmentId = overdueAssignment.Id,
                BodyText = "My reflection",
                Status = SubmissionStatus.Submitted,
                SubmittedAt = now.AddHours(-3),
            };
            overdueSubmissionId = overdueSubmission.Id;
            db.Submissions.AddRange(
                overdueSubmission,
                new Submission
                {
                    YouthUserId = scenario.OtherYouth,
                    AssignmentId = otherAssignment.Id,
                    Status = SubmissionStatus.Submitted,
                    SubmittedAt = now.AddHours(-4),
                });

            var quiz = new Quiz { PassMark = 70 };
            db.Quizzes.Add(quiz);
            var attempt = new QuizAttempt
            {
                YouthUserId = scenario.YouthB,
                QuizId = quiz.Id,
                AttemptNumber = 1,
                IsSubmitted = true,
                AutoScore = 80,
                SubmittedAt = now.AddHours(-1),
            };
            pendingQuizAttemptId = attempt.Id;
            db.QuizAttempts.Add(attempt);

            db.Flags.AddRange(
                new Flag
                {
                    YouthUserId = scenario.YouthB,
                    Reason = "Needs a follow-up",
                    FlaggedByUserId = scenario.MentorId,
                    FlaggedAt = now,
                },
                new Flag
                {
                    YouthUserId = scenario.OtherYouth,
                    Reason = "Must remain private",
                    FlaggedByUserId = scenario.MentorId,
                    FlaggedAt = now,
                });

            db.LessonProgress.AddRange(
                new LessonProgress
                {
                    YouthUserId = scenario.YouthA,
                    LessonId = Guid.NewGuid(),
                    Status = ProgressStatus.Completed,
                },
                new LessonProgress
                {
                    YouthUserId = scenario.YouthA,
                    LessonId = Guid.NewGuid(),
                    Status = ProgressStatus.InProgress,
                },
                new LessonProgress
                {
                    YouthUserId = scenario.YouthB,
                    LessonId = Guid.NewGuid(),
                    Status = ProgressStatus.NotStarted,
                });
            await db.SaveChangesAsync();
        }

        var token = await SignInAsync(scenario.MentorEmail);
        var dashboardResponse = await GetAsync("/api/mentor/dashboard", token);
        var dashboard = await dashboardResponse.Content.ReadFromJsonAsync<DashboardAcceptance>();
        Assert.Multiple(() =>
        {
            Assert.That(dashboardResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(dashboard!.YouthCount, Is.EqualTo(2));
            Assert.That(dashboard.PendingReviews, Is.EqualTo(2));
            Assert.That(dashboard.OverdueReviews, Is.EqualTo(1));
            Assert.That(dashboard.AverageProgress, Is.EqualTo(0.25).Within(0.001));
            Assert.That(dashboard.AtRiskCount, Is.EqualTo(1));
        });

        var reviewsResponse = await GetAsync("/api/mentor/reviews?take=5", token);
        Assert.That(reviewsResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var reviews = await reviewsResponse.Content.ReadFromJsonAsync<List<ReviewAcceptance>>();
        var reviewList = reviews!;
        Assert.Multiple(() =>
        {
            Assert.That(reviewList, Has.Count.EqualTo(2));
            Assert.That(reviewList.Select(item => item.YouthId), Does.Not.Contain(scenario.OtherYouth));
            Assert.That(reviewList.Select(item => item.ReviewId), Does.Contain(overdueSubmissionId));
            Assert.That(reviewList.Select(item => item.ReviewId), Does.Contain(pendingQuizAttemptId));
            var overdue = reviewList.Single(item => item.ReviewId == overdueSubmissionId);
            Assert.That(overdue.YouthName, Is.EqualTo("Aaron Youth"));
            Assert.That(overdue.ItemType, Is.EqualTo("Assignment"));
            Assert.That(overdue.ItemTitle, Is.EqualTo("Overdue reflection"));
            Assert.That(overdue.Overdue, Is.True);
            Assert.That(overdue.ReviewPath, Is.EqualTo($"/mentor/reviews/submissions/{overdueSubmissionId}"));
        });

        var atRiskResponse = await GetAsync("/api/mentor/youth?filter=at-risk", token);
        var atRisk = await atRiskResponse.Content.ReadFromJsonAsync<List<RosterAcceptance>>();
        Assert.That(atRisk, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(atRisk![0].Id, Is.EqualTo(scenario.YouthB));
            Assert.That(atRisk[0].Status, Is.EqualTo("At risk"));
            Assert.That(atRisk[0].Progress, Is.EqualTo(0));
        });

        var newResponse = await GetAsync("/api/mentor/youth?filter=new", token);
        var newYouth = await newResponse.Content.ReadFromJsonAsync<List<RosterAcceptance>>();
        Assert.That(newYouth!.Select(item => item.Id), Is.EquivalentTo(new[] { scenario.YouthB }));

        var searchResponse = await GetAsync("/api/mentor/youth?search=Aaron", token);
        var searched = await searchResponse.Content.ReadFromJsonAsync<List<RosterAcceptance>>();
        Assert.That(searched!.Select(item => item.Id), Is.EquivalentTo(new[] { scenario.YouthA }));

        var emptyResponse = await GetAsync("/api/mentor/youth?search=NoSuchYouth", token);
        var empty = await emptyResponse.Content.ReadFromJsonAsync<List<RosterAcceptance>>();
        Assert.That(empty, Is.Empty);
    }

    [Test]
    [Property("Req", "L2-MENTOR-002")]
    [Property("Req", "L2-AUTH-010")]
    public async Task Youth_detail_is_allowed_for_an_assigned_youth_and_forbidden_for_another_mentors_youth()
    {
        // Traces to: L2-MENTOR-002, L2-AUTH-010
        var scenario = await SeedMentorWithYouthAsync();
        var token = await SignInAsync(scenario.MentorEmail);

        var allowed = await GetAsync($"/api/mentor/youth/{scenario.YouthA}", token);
        Assert.That(allowed.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var detail = await allowed.Content.ReadFromJsonAsync<YouthDetail>();
        Assert.That(detail!.Id, Is.EqualTo(scenario.YouthA));

        var forbidden = await GetAsync($"/api/mentor/youth/{scenario.OtherYouth}", token);
        Assert.That(forbidden.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }
}
