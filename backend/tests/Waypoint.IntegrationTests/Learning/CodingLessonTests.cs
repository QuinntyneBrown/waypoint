using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Waypoint.Application.Authentication.Auth;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Infrastructure;

namespace Waypoint.IntegrationTests.Learning;

// Acceptance Test
// Traces to: L2-LEARN-008 (interactive coding lessons and reviewable coding projects)
[TestFixture]
[Property("Req", "L2-LEARN-008")]
public sealed class CodingLessonTests : IntegrationTestBase
{
    private sealed record Seed(
        Guid LessonId,
        Guid AssignmentId,
        Guid FirstStepId,
        Guid SecondStepId,
        Guid BadgeId,
        Guid YouthId,
        string Token);

    private async Task<Seed> SeedAsync()
    {
        var track = new Track { Name = "Coding", Key = $"coding-{Guid.NewGuid():N}", Status = PublishStatus.Published };
        var module = new Module { TrackId = track.Id, Name = "Web basics", Order = 1, Status = PublishStatus.Published };
        var lesson = new Lesson
        {
            ModuleId = module.Id,
            Title = "Build a welcome card",
            TeachingBody = "Use HTML to build a card, then preview your work.",
            StarterCode = "<h1>Hello!</h1>",
            IsCodingLesson = true,
            Order = 1,
            Status = PublishStatus.Published,
        };
        var assignment = new Assignment
        {
            ModuleId = module.Id,
            LessonId = lesson.Id,
            Title = "Welcome card project",
            TaskDescription = "Submit the finished welcome card.",
            SubmissionTypes = "text",
            RequireMentorApproval = true,
            TotalPoints = 10,
        };
        var first = new CodingLessonStep
        {
            LessonId = lesson.Id,
            Order = 1,
            Title = "Add a heading",
            Instructions = "Add an h1 with your welcome message.",
            CodeSample = "<h1>Welcome</h1>",
            ChecklistLabel = "My card has a heading",
        };
        var second = new CodingLessonStep
        {
            LessonId = lesson.Id,
            Order = 2,
            Title = "Add a paragraph",
            Instructions = "Add a short paragraph below the heading.",
            CodeSample = "<p>I am learning to code.</p>",
            ChecklistLabel = "My card has a paragraph",
        };
        var badge = new Badge
        {
            Name = "Code Starter",
            Icon = "code",
            CriteriaKind = BadgeCriteriaKind.Auto,
            AutoRule = "coding-checklist-completed",
            TargetRefId = lesson.Id,
        };
        var cohort = new Cohort { Name = "Coding cohort", TrackId = track.Id };
        var email = $"coding.youth.{Guid.NewGuid():N}@example.com";
        Guid youthId;

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var youth = new User
            {
                Email = email,
                DisplayName = "Coding Youth",
                Role = Roles.Youth,
                Status = UserStatus.Active,
                CohortId = cohort.Id,
                PasswordHash = hasher.Hash("Passw0rd!"),
            };
            youthId = youth.Id;
            db.AddRange(track, module, lesson, assignment, first, second, badge, cohort, youth);
            await db.SaveChangesAsync();
        }

        var signIn = await Client.PostAsJsonAsync("/api/auth/sign-in", new { email, password = "Passw0rd!" });
        signIn.EnsureSuccessStatusCode();
        var auth = await signIn.Content.ReadFromJsonAsync<AuthResult>();
        return new Seed(lesson.Id, assignment.Id, first.Id, second.Id, badge.Id, youthId, auth!.AccessToken);
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, object? body, string token)
    {
        var request = new HttpRequestMessage(method, url);
        if (body is not null) request.Content = JsonContent.Create(body);
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    [Test]
    public async Task Lesson_delivers_ordered_steps_samples_starter_workspace_and_badge_criteria()
    {
        var seed = await SeedAsync();

        var response = await SendAsync(HttpMethod.Get, $"/api/me/coding-lessons/{seed.LessonId}", null, seed.Token);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var view = await response.Content.ReadFromJsonAsync<CodingLessonDto>();

        Assert.Multiple(() =>
        {
            Assert.That(view!.Title, Is.EqualTo("Build a welcome card"));
            Assert.That(view.AssignmentId, Is.EqualTo(seed.AssignmentId));
            Assert.That(view.Code, Is.EqualTo("<h1>Hello!</h1>"));
            Assert.That(view.Steps.Select(step => step.Title), Is.EqualTo(new[] { "Add a heading", "Add a paragraph" }));
            Assert.That(view.Steps[0].CodeSample, Does.Contain("<h1>"));
            Assert.That(view.CheckedStepIds, Is.Empty);
            Assert.That(view.Badge.Name, Is.EqualTo("Code Starter"));
            Assert.That(view.Badge.CompletedCriteria, Is.Zero);
            Assert.That(view.Badge.TotalCriteria, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task Workspace_and_checklist_resume_and_completion_records_progress_and_badge_once()
    {
        var seed = await SeedAsync();

        var invalid = await SendAsync(
            HttpMethod.Put,
            $"/api/me/coding-lessons/{seed.LessonId}/progress",
            new { code = "<h1>Changed</h1>", checkedStepIds = new[] { Guid.NewGuid() } },
            seed.Token);
        Assert.That(invalid.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var partial = await SendAsync(
            HttpMethod.Put,
            $"/api/me/coding-lessons/{seed.LessonId}/progress",
            new { code = "<h1>Welcome, Toronto!</h1>", checkedStepIds = new[] { seed.FirstStepId } },
            seed.Token);
        Assert.That(partial.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var resumed = await (await SendAsync(
            HttpMethod.Get,
            $"/api/me/coding-lessons/{seed.LessonId}",
            null,
            seed.Token)).Content.ReadFromJsonAsync<CodingLessonDto>();
        Assert.Multiple(() =>
        {
            Assert.That(resumed!.Code, Does.Contain("Toronto"));
            Assert.That(resumed.CheckedStepIds, Is.EquivalentTo(new[] { seed.FirstStepId }));
            Assert.That(resumed.CompletionPercent, Is.EqualTo(50));
            Assert.That(resumed.Badge.Earned, Is.False);
        });

        var complete = await SendAsync(
            HttpMethod.Put,
            $"/api/me/coding-lessons/{seed.LessonId}/progress",
            new { code = "<h1>Welcome, Toronto!</h1><p>I am learning to code.</p>", checkedStepIds = new[] { seed.FirstStepId, seed.SecondStepId } },
            seed.Token);
        Assert.That(complete.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        await SendAsync(
            HttpMethod.Put,
            $"/api/me/coding-lessons/{seed.LessonId}/progress",
            new { code = "<h1>Welcome, Toronto!</h1><p>I am learning to code.</p>", checkedStepIds = new[] { seed.FirstStepId, seed.SecondStepId } },
            seed.Token);

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Multiple(() =>
        {
            Assert.That(db.LessonProgress.Single(item => item.YouthUserId == seed.YouthId && item.LessonId == seed.LessonId).Status,
                Is.EqualTo(ProgressStatus.Completed));
            Assert.That(db.BadgeAwards.Count(item => item.YouthUserId == seed.YouthId && item.BadgeId == seed.BadgeId), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Project_with_optional_mentor_note_enters_assignment_review_lifecycle()
    {
        var seed = await SeedAsync();
        var submitted = await SendAsync(
            HttpMethod.Post,
            $"/api/me/coding-lessons/{seed.LessonId}/submissions",
            new { code = "<h1>My finished project</h1>", mentorNote = "Please check the heading colour.", attest = true },
            seed.Token);
        Assert.That(submitted.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var result = await submitted.Content.ReadFromJsonAsync<SubmissionResultDto>();

        var status = await SendAsync(HttpMethod.Get, $"/api/me/submissions/{result!.Id}", null, seed.Token);
        var view = await status.Content.ReadFromJsonAsync<SubmissionStatusDto>();
        Assert.Multiple(() =>
        {
            Assert.That(view!.Status, Is.EqualTo("Submitted"));
            Assert.That(view.IsCodingProject, Is.True);
            Assert.That(view.BodyText, Does.Contain("finished project"));
            Assert.That(view.YouthNote, Is.EqualTo("Please check the heading colour."));
            Assert.That(view.Timeline.Select(item => item.Type), Does.Contain("UnderReview"));
        });
    }

    private sealed record CodingLessonDto(
        Guid LessonId,
        Guid AssignmentId,
        string Title,
        string Code,
        IReadOnlyList<CodingStepDto> Steps,
        IReadOnlyList<Guid> CheckedStepIds,
        int CompletionPercent,
        CodingBadgeDto Badge);
    private sealed record CodingStepDto(Guid Id, int Order, string Title, string Instructions, string CodeSample, string ChecklistLabel);
    private sealed record CodingBadgeDto(Guid Id, string Name, string Icon, int CompletedCriteria, int TotalCriteria, bool Earned);
    private sealed record SubmissionResultDto(Guid Id);
    private sealed record TimelineDto(string Type);
    private sealed record SubmissionStatusDto(
        Guid Id,
        string Status,
        bool IsCodingProject,
        string? BodyText,
        string? YouthNote,
        IReadOnlyList<TimelineDto> Timeline);
}
