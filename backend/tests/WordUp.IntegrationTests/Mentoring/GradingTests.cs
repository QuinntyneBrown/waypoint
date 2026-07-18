using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Abstractions;
using WordUp.Domain;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Mentoring;

// Acceptance Test
// Traces to: L2-MENTOR-003 (submission grading), L2-MENTOR-004 (quiz approval/override),
//            L2-LEARN-005 (quiz-passed auto-badge on approval)
[TestFixture]
[Property("Req", "L2-MENTOR-003")]
public sealed class GradingTests : IntegrationTestBase
{
    private async Task<HttpResponseMessage> PostAsync(string url, object body, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> GetAsync(string url, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    private async Task<string> SignInAsync(string email)
    {
        var signIn = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password = "Passw0rd!" });
        signIn.EnsureSuccessStatusCode();
        var auth = await signIn.Content.ReadFromJsonAsync<WordUp.Application.Auth.AuthResult>();
        return auth!.AccessToken;
    }

    // Seeds a mentor + a youth assigned to that mentor. Returns their ids and signed-in tokens.
    private async Task<(Guid mentorId, string mentorToken, Guid youthId, string youthToken)> SeedMentorAndYouthAsync()
    {
        var mentorEmail = $"mentor.{Guid.NewGuid():N}@example.com";
        var youthEmail = $"youth.{Guid.NewGuid():N}@example.com";
        Guid mentorId, youthId;

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            var mentor = new User
            {
                Email = mentorEmail,
                DisplayName = "Mentor",
                Role = Roles.Mentor,
                Status = UserStatus.Active,
                PasswordHash = hasher.Hash("Passw0rd!"),
            };
            db.Users.Add(mentor);
            await db.SaveChangesAsync();
            mentorId = mentor.Id;

            var youth = new User
            {
                Email = youthEmail,
                DisplayName = "Youth",
                Role = Roles.Youth,
                Status = UserStatus.Active,
                MentorId = mentorId,
                PasswordHash = hasher.Hash("Passw0rd!"),
            };
            db.Users.Add(youth);
            await db.SaveChangesAsync();
            youthId = youth.Id;
        }

        return (mentorId, await SignInAsync(mentorEmail), youthId, await SignInAsync(youthEmail));
    }

    private async Task<Guid> SeedSubmissionAsync(Guid youthId)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var assignment = new Assignment
        {
            Title = "Reflection",
            TaskDescription = "Reflect on the lesson.",
            SubmissionTypes = "text",
            TotalPoints = 10,
        };
        db.Assignments.Add(assignment);
        var submission = new Submission
        {
            YouthUserId = youthId,
            AssignmentId = assignment.Id,
            BodyText = "My work",
            Status = SubmissionStatus.Submitted,
        };
        db.Submissions.Add(submission);
        await db.SaveChangesAsync();
        return submission.Id;
    }

    [Test]
    [Property("Req", "L2-MENTOR-003")]
    public async Task Assigned_mentor_approves_but_an_unassigned_mentor_is_forbidden()
    {
        var (_, mentorToken, youthId, youthToken) = await SeedMentorAndYouthAsync();
        var submissionId = await SeedSubmissionAsync(youthId);

        // Traces to: L2-MENTOR-003 (mentor approves -> Approved + points visible to youth)
        var grade = await PostAsync(
            $"/api/mentor/submissions/{submissionId}/grade",
            new { decision = "approve", totalAwarded = 9, comment = "Great work", guardianNotified = false },
            mentorToken);
        Assert.That(grade.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var status = await GetAsync($"/api/me/submissions/{submissionId}", youthToken);
        var view = await status.Content.ReadFromJsonAsync<SubmissionStatusDto>();
        Assert.That(view!.Status, Is.EqualTo(SubmissionStatus.Approved.ToString()));
        Assert.That(view.TotalAwarded, Is.EqualTo(9));

        // Traces to: L2-MENTOR-003 AC5 (a mentor not assigned to this youth cannot grade -> 403)
        var strangerToken = await CreateUserAndGetTokenAsync(Roles.Mentor);
        var otherSubmissionId = await SeedSubmissionAsync(youthId);
        var forbidden = await PostAsync(
            $"/api/mentor/submissions/{otherSubmissionId}/grade",
            new { decision = "approve", totalAwarded = 5, guardianNotified = false },
            strangerToken);
        Assert.That(forbidden.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    [Property("Req", "L2-MENTOR-003")]
    [Property("Req", "L2-LEARN-007")]
    public async Task Request_revision_requires_a_comment_and_the_youth_can_then_resubmit()
    {
        var (_, mentorToken, youthId, youthToken) = await SeedMentorAndYouthAsync();
        var submissionId = await SeedSubmissionAsync(youthId);

        // Traces to: L2-MENTOR-003 AC2 (request-revision without a comment -> 400)
        var noComment = await PostAsync(
            $"/api/mentor/submissions/{submissionId}/grade",
            new { decision = "request-revision", guardianNotified = true },
            mentorToken);
        Assert.That(noComment.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var withComment = await PostAsync(
            $"/api/mentor/submissions/{submissionId}/grade",
            new { decision = "request-revision", comment = "Please add scripture.", guardianNotified = true },
            mentorToken);
        Assert.That(withComment.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var afterRevision = await GetAsync($"/api/me/submissions/{submissionId}", youthToken);
        var revView = await afterRevision.Content.ReadFromJsonAsync<SubmissionStatusDto>();
        Assert.That(revView!.Status, Is.EqualTo(SubmissionStatus.NeedsRevision.ToString()));

        // Traces to: L2-LEARN-007 AC3 (youth resubmits -> UnderReview)
        var resubmit = await PostAsync(
            $"/api/me/submissions/{submissionId}/resubmit",
            new { bodyText = "Now with scripture" },
            youthToken);
        Assert.That(resubmit.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var afterResubmit = await GetAsync($"/api/me/submissions/{submissionId}", youthToken);
        var reView = await afterResubmit.Content.ReadFromJsonAsync<SubmissionStatusDto>();
        Assert.That(reView!.Status, Is.EqualTo(SubmissionStatus.UnderReview.ToString()));
    }

    [Test]
    [Property("Req", "L2-MENTOR-004")]
    [Property("Req", "L2-LEARN-005")]
    public async Task Approving_a_passing_quiz_attempt_records_the_score_and_awards_the_quiz_badge()
    {
        var (_, mentorToken, youthId, _) = await SeedMentorAndYouthAsync();

        Guid attemptId, badgeId;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var quiz = new Quiz { PassMark = 70, AttemptsAllowed = 2 };
            db.Quizzes.Add(quiz);
            var attempt = new QuizAttempt
            {
                YouthUserId = youthId,
                QuizId = quiz.Id,
                AttemptNumber = 1,
                AutoScore = 80,
                Passed = true,
                IsSubmitted = true,
                SubmittedAt = DateTime.UtcNow,
            };
            db.QuizAttempts.Add(attempt);
            var badge = new Badge
            {
                Name = "Quiz Ace",
                Icon = "trophy",
                CriteriaKind = BadgeCriteriaKind.Auto,
                AutoRule = "quiz-passed",
                TargetRefId = quiz.Id,
            };
            db.Badges.Add(badge);
            await db.SaveChangesAsync();
            attemptId = attempt.Id;
            badgeId = badge.Id;
        }

        // Traces to: L2-MENTOR-004 (mentor approves the attempt)
        var approve = await PostAsync($"/api/mentor/attempts/{attemptId}/approve", new { }, mentorToken);
        Assert.That(approve.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Traces to: L2-MENTOR-004 AC4 + L2-LEARN-005 AC2 (approved score recorded; badge awarded)
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var attempt = await db.QuizAttempts.FirstAsync(a => a.Id == attemptId);
            Assert.That(attempt.ApprovedScore, Is.EqualTo(80));
            Assert.That(attempt.ApprovedAt, Is.Not.Null);

            var awarded = await db.BadgeAwards
                .AnyAsync(a => a.BadgeId == badgeId && a.YouthUserId == youthId);
            Assert.That(awarded, Is.True, "a quiz-passed badge should have been awarded");
        }
    }

    private sealed record SubmissionStatusDto(
        Guid Id, string Status, string? MentorComment, int? TotalAwarded, DateTime SubmittedAt, DateTime? ReviewedAt);
}
