// Acceptance Test
// Traces to: L2-NFR-006 (optimistic concurrency: stale save rejected with 409, version advances)
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Application.Shared.Abstractions;
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

    private async Task<(Guid SubmissionId, Guid CriterionId, string YouthToken)> SeedSubmissionAsync()
    {
        const string password = "Passw0rd!";
        var email = $"concurrent.youth.{Guid.NewGuid():N}@example.com";
        Guid submissionId;
        Guid criterionId;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var youth = new User
            {
                Email = email,
                DisplayName = "Concurrent Youth",
                Role = Roles.Youth,
                Status = UserStatus.Active,
                PasswordHash = hasher.Hash(password),
            };
            var assignment = new Assignment
            {
                Title = "Concurrent review",
                TaskDescription = "Explain what you learned.",
                SubmissionTypes = "text",
                TotalPoints = 10,
            };
            var criterion = new RubricCriterion
            {
                AssignmentId = assignment.Id,
                Description = "Clear reflection",
                Points = 10,
                Order = 1,
            };
            var submission = new Submission
            {
                YouthUserId = youth.Id,
                AssignmentId = assignment.Id,
                BodyText = "My original response",
                Status = SubmissionStatus.UnderReview,
            };
            db.Users.Add(youth);
            db.Assignments.Add(assignment);
            db.RubricCriteria.Add(criterion);
            db.Submissions.Add(submission);
            await db.SaveChangesAsync();
            submissionId = submission.Id;
            criterionId = criterion.Id;
        }

        var signIn = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password });
        signIn.EnsureSuccessStatusCode();
        var auth = await signIn.Content.ReadFromJsonAsync<TokenDto>();
        return (submissionId, criterionId, auth!.AccessToken);
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

    [Test]
    [Property("Req", "L2-NFR-006")]
    public async Task A_stale_submission_grade_is_rejected_and_preserves_the_winning_review()
    {
        // Traces to: L2-NFR-006 (submission optimistic concurrency and lost-update prevention)
        var token = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var (submissionId, criterionId, _) = await SeedSubmissionAsync();

        var initialRead = await SendAsync(
            HttpMethod.Get,
            $"/api/mentor/submissions/{submissionId}",
            token: token);
        var initial = await initialRead.Content.ReadFromJsonAsync<SubmissionReviewDto>();
        Assert.That(initial!.Version, Is.Not.Null.And.Not.Empty);

        var winningSave = await SendAsync(
            HttpMethod.Post,
            $"/api/mentor/submissions/{submissionId}/grade",
            new
            {
                decision = "save-draft",
                rubricScoresJson = $"{{\"{criterionId}\":7}}",
                comment = "Winning editor feedback",
                guardianNotified = false,
                expectedVersion = initial.Version,
            },
            token);
        Assert.That(winningSave.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var afterWinningSave = await SendAsync(
            HttpMethod.Get,
            $"/api/mentor/submissions/{submissionId}",
            token: token);
        var current = await afterWinningSave.Content.ReadFromJsonAsync<SubmissionReviewDto>();
        Assert.Multiple(() =>
        {
            Assert.That(current!.Version, Is.Not.EqualTo(initial.Version));
            Assert.That(current.MentorComment, Is.EqualTo("Winning editor feedback"));
        });

        var staleSave = await SendAsync(
            HttpMethod.Post,
            $"/api/mentor/submissions/{submissionId}/grade",
            new
            {
                decision = "save-draft",
                rubricScoresJson = $"{{\"{criterionId}\":3}}",
                comment = "Stale overwrite",
                guardianNotified = false,
                expectedVersion = initial.Version,
            },
            token);
        Assert.That(staleSave.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

        var finalRead = await SendAsync(
            HttpMethod.Get,
            $"/api/mentor/submissions/{submissionId}",
            token: token);
        var final = await finalRead.Content.ReadFromJsonAsync<SubmissionReviewDto>();
        Assert.Multiple(() =>
        {
            Assert.That(final!.Version, Is.EqualTo(current!.Version));
            Assert.That(final.MentorComment, Is.EqualTo("Winning editor feedback"));
            Assert.That(final.Rubric.Single().AwardedPoints, Is.EqualTo(7));
        });
    }

    [Test]
    [Property("Req", "L2-NFR-006")]
    public async Task A_stale_youth_resubmission_is_rejected_and_preserves_the_newer_review()
    {
        // Traces to: L2-NFR-006 (submission optimistic concurrency and reconciliation)
        var administratorToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var (submissionId, _, youthToken) = await SeedSubmissionAsync();

        var openedByMentor = await SendAsync(
            HttpMethod.Get,
            $"/api/mentor/submissions/{submissionId}",
            token: administratorToken);
        var mentorView = await openedByMentor.Content.ReadFromJsonAsync<SubmissionReviewDto>();
        var revisionRequest = await SendAsync(
            HttpMethod.Post,
            $"/api/mentor/submissions/{submissionId}/grade",
            new
            {
                decision = "request-revision",
                comment = "Initial feedback",
                guardianNotified = false,
                expectedVersion = mentorView!.Version,
            },
            administratorToken);
        Assert.That(revisionRequest.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var openedByYouth = await SendAsync(
            HttpMethod.Get,
            $"/api/me/submissions/{submissionId}",
            token: youthToken);
        var youthView = await openedByYouth.Content.ReadFromJsonAsync<SubmissionStatusDto>();
        Assert.That(youthView!.Version, Is.Not.Null.And.Not.Empty);

        var reopenedByMentor = await SendAsync(
            HttpMethod.Get,
            $"/api/mentor/submissions/{submissionId}",
            token: administratorToken);
        var currentMentorView = await reopenedByMentor.Content.ReadFromJsonAsync<SubmissionReviewDto>();
        var newerReview = await SendAsync(
            HttpMethod.Post,
            $"/api/mentor/submissions/{submissionId}/grade",
            new
            {
                decision = "save-draft",
                comment = "Newer mentor feedback",
                guardianNotified = false,
                expectedVersion = currentMentorView!.Version,
            },
            administratorToken);
        Assert.That(newerReview.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var staleResubmission = await SendAsync(
            HttpMethod.Post,
            $"/api/me/submissions/{submissionId}/resubmit",
            new
            {
                bodyText = "Stale youth response",
                expectedVersion = youthView.Version,
            },
            youthToken);
        Assert.That(staleResubmission.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

        var finalRead = await SendAsync(
            HttpMethod.Get,
            $"/api/me/submissions/{submissionId}",
            token: youthToken);
        var final = await finalRead.Content.ReadFromJsonAsync<SubmissionStatusDto>();
        Assert.Multiple(() =>
        {
            Assert.That(final!.Version, Is.Not.EqualTo(youthView.Version));
            Assert.That(final.Status, Is.EqualTo(SubmissionStatus.NeedsRevision.ToString()));
            Assert.That(final.BodyText, Is.EqualTo("My original response"));
            Assert.That(final.MentorComment, Is.EqualTo("Newer mentor feedback"));
        });
    }

    [Test]
    [Property("Req", "L2-NFR-006")]
    public async Task Submission_updates_require_the_version_that_the_editor_read()
    {
        // Traces to: L2-NFR-006 (all Submission aggregate updates are concurrency protected)
        var administratorToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var (submissionId, _, youthToken) = await SeedSubmissionAsync();

        var gradeWithoutVersion = await SendAsync(
            HttpMethod.Post,
            $"/api/mentor/submissions/{submissionId}/grade",
            new
            {
                decision = "request-revision",
                comment = "Please revise this response.",
                guardianNotified = false,
            },
            administratorToken);
        Assert.That(gradeWithoutVersion.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var openedByMentor = await SendAsync(
            HttpMethod.Get,
            $"/api/mentor/submissions/{submissionId}",
            token: administratorToken);
        var mentorView = await openedByMentor.Content.ReadFromJsonAsync<SubmissionReviewDto>();
        var revisionRequest = await SendAsync(
            HttpMethod.Post,
            $"/api/mentor/submissions/{submissionId}/grade",
            new
            {
                decision = "request-revision",
                comment = "Please revise this response.",
                guardianNotified = false,
                expectedVersion = mentorView!.Version,
            },
            administratorToken);
        Assert.That(revisionRequest.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var resubmitWithoutVersion = await SendAsync(
            HttpMethod.Post,
            $"/api/me/submissions/{submissionId}/resubmit",
            new { bodyText = "My unversioned revision" },
            youthToken);
        Assert.That(resubmitWithoutVersion.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    private sealed record MarkDto(Guid YouthUserId, string Status, string? Note, DateTime RecordedAt, string? Version);
    private sealed record SubmissionReviewDto(
        string? Version,
        string? MentorComment,
        IReadOnlyList<SubmissionCriterionDto> Rubric);
    private sealed record SubmissionCriterionDto(Guid CriterionId, int? AwardedPoints);
    private sealed record SubmissionStatusDto(
        string? Version,
        string Status,
        string? BodyText,
        string? MentorComment);
    private sealed record TokenDto(string AccessToken);
}
