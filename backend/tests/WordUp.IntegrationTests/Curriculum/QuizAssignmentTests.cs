using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Curriculum;

// Acceptance Test
// Traces to: L2-CURRIC-003 (quiz authoring), L2-CURRIC-004 (assignment authoring)
[TestFixture]
[Property("Req", "L2-CURRIC-003")]
[Property("Req", "L2-CURRIC-004")]
public sealed class QuizAssignmentTests : IntegrationTestBase
{
    private async Task<HttpResponseMessage> PostAsync(string url, object body, string? token = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
        if (token is not null)
        {
            Authorize(request, token);
        }
        return await Client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> GetAsync(string url, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> PutAsync(string url, object body, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, url) { Content = JsonContent.Create(body) };
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    private sealed record IdDto(Guid Id);

    private sealed record QuizDetailDto(Guid Id, int QuestionCount);

    private sealed record AssignmentResultDto(Guid Id, int TotalPoints);

    [Test]
    [Property("Req", "L2-CURRIC-003")]
    public async Task Admin_authors_a_quiz_with_a_question_and_a_youth_cannot()
    {
        // Traces to: L2-CURRIC-003
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);

        var create = await PostAsync("/api/quizzes", new
        {
            moduleId = (Guid?)null,
            lessonId = (Guid?)null,
            passMark = 70,
            attemptsAllowed = 2,
            feedbackMode = "show-correct",
            shuffle = false,
            requiredToCompleteModule = true,
        }, adminToken);
        Assert.That(create.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var quizId = (await create.Content.ReadFromJsonAsync<IdDto>())!.Id;

        var addQuestion = await PostAsync($"/api/quizzes/{quizId}/questions", new
        {
            text = "What is the greatest commandment?",
            type = QuestionType.MultipleChoice,
            options = new[] { "Love God", "Love money", "Sleep in" },
            correctIndexes = new[] { 0 },
        }, adminToken);
        Assert.That(addQuestion.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var get = await GetAsync($"/api/quizzes/{quizId}", adminToken);
        Assert.That(get.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var detail = await get.Content.ReadFromJsonAsync<QuizDetailDto>();
        Assert.That(detail!.QuestionCount, Is.EqualTo(1));

        // A youth may not author curriculum.
        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);
        var forbidden = await PostAsync("/api/quizzes", new
        {
            passMark = 50,
            attemptsAllowed = 1,
            feedbackMode = "score-only",
            shuffle = false,
            requiredToCompleteModule = false,
        }, youthToken);
        Assert.That(forbidden.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    [Property("Req", "L2-CURRIC-004")]
    public async Task Assignment_totals_rubric_points_and_requires_a_submission_type()
    {
        // Traces to: L2-CURRIC-004
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);

        var create = await PostAsync("/api/assignments", new
        {
            moduleId = (Guid?)null,
            lessonId = (Guid?)null,
            title = "Memory Verse Reflection",
            taskDescription = "Write a short reflection on this week's memory verse.",
            submissionTypes = new[] { "text", "photo" },
            dueDate = (DateTime?)null,
            gracePeriodDays = 2,
            allowLate = true,
            requireMentorApproval = true,
            rubric = new[]
            {
                new { description = "Depth of reflection", points = 10 },
                new { description = "Scripture accuracy", points = 5 },
            },
        }, adminToken);
        Assert.That(create.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var result = await create.Content.ReadFromJsonAsync<AssignmentResultDto>();
        Assert.That(result!.TotalPoints, Is.EqualTo(15));

        // An assignment must accept at least one submission type.
        var invalid = await PostAsync("/api/assignments", new
        {
            title = "Bad Assignment",
            taskDescription = "No submission types.",
            submissionTypes = Array.Empty<string>(),
            gracePeriodDays = 0,
            allowLate = false,
            requireMentorApproval = false,
            rubric = Array.Empty<object>(),
        }, adminToken);
        Assert.That(invalid.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    [Property("Req", "L2-CURRIC-003")]
    public async Task Admin_configures_validated_typed_questions_and_persists_their_exact_order()
    {
        // Traces to: L2-CURRIC-003 (AC1 typed answers, AC2 settings, AC3 ordering)
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var trackId = (await (await PostAsync("/api/tracks", new
        {
            name = "Quiz Builder",
            key = $"quiz-builder-{Guid.NewGuid():N}",
        }, adminToken)).Content.ReadFromJsonAsync<IdDto>())!.Id;
        var moduleId = (await (await PostAsync("/api/modules", new
        {
            trackId,
            name = "Quiz Module",
        }, adminToken)).Content.ReadFromJsonAsync<IdDto>())!.Id;

        var invalidSettings = await PostAsync("/api/quizzes", new
        {
            moduleId,
            passMark = 101,
            attemptsAllowed = 0,
            feedbackMode = "unknown",
            shuffle = false,
            requiredToCompleteModule = true,
        }, adminToken);
        Assert.That(invalidSettings.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var quizId = (await (await PostAsync("/api/quizzes", new
        {
            moduleId,
            passMark = 80,
            attemptsAllowed = 3,
            feedbackMode = "after-all-attempts",
            shuffle = true,
            requiredToCompleteModule = true,
        }, adminToken)).Content.ReadFromJsonAsync<IdDto>())!.Id;
        var choiceId = (await (await PostAsync($"/api/quizzes/{quizId}/questions", new
        {
            text = "Which response shows service?",
            type = QuestionType.MultipleChoice,
            options = new[] { "Helping", "Ignoring" },
            correctIndexes = new[] { 0 },
            correctAnswers = Array.Empty<string>(),
        }, adminToken)).Content.ReadFromJsonAsync<IdDto>())!.Id;
        var shortId = (await (await PostAsync($"/api/quizzes/{quizId}/questions", new
        {
            text = "Complete the phrase: saved by ____.",
            type = QuestionType.ShortAnswer,
            options = Array.Empty<string>(),
            correctIndexes = Array.Empty<int>(),
            correctAnswers = new[] { "grace", "God's grace" },
        }, adminToken)).Content.ReadFromJsonAsync<IdDto>())!.Id;

        var invalidChoice = await PostAsync($"/api/quizzes/{quizId}/questions", new
        {
            text = "Invalid choice",
            type = QuestionType.MultipleChoice,
            options = new[] { "Only option" },
            correctIndexes = new[] { 4 },
            correctAnswers = Array.Empty<string>(),
        }, adminToken);
        Assert.That(invalidChoice.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var reorder = await PostAsync($"/api/quizzes/{quizId}/questions/reorder", new
        {
            orderedQuestionIds = new[] { shortId, choiceId },
        }, adminToken);
        Assert.That(reorder.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        var partial = await PostAsync($"/api/quizzes/{quizId}/questions/reorder", new
        {
            orderedQuestionIds = new[] { choiceId },
        }, adminToken);
        Assert.That(partial.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

        var update = await PutAsync($"/api/quizzes/{quizId}", new
        {
            moduleId,
            lessonId = (Guid?)null,
            passMark = 75,
            attemptsAllowed = 2,
            feedbackMode = "score-only",
            shuffle = false,
            requiredToCompleteModule = false,
        }, adminToken);
        Assert.That(update.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var detail = await (await GetAsync($"/api/quizzes/{quizId}", adminToken))
            .Content.ReadFromJsonAsync<CompleteQuizDetailDto>();
        Assert.Multiple(() =>
        {
            Assert.That(detail!.PassMark, Is.EqualTo(75));
            Assert.That(detail.AttemptsAllowed, Is.EqualTo(2));
            Assert.That(detail.FeedbackMode, Is.EqualTo("score-only"));
            Assert.That(detail.Questions.Select(item => item.Id), Is.EqualTo(new[] { shortId, choiceId }));
            Assert.That(detail.Questions[0].CorrectAnswers, Is.EqualTo(new[] { "grace", "God's grace" }));
            Assert.That(detail.Questions[1].CorrectIndexes, Is.EqualTo(new[] { 0 }));
        });
    }

    [Test]
    [Property("Req", "L2-CURRIC-004")]
    public async Task Publishing_a_validated_assignment_assigns_it_once_to_each_youth_in_the_track_cohorts()
    {
        // Traces to: L2-CURRIC-004 (AC1 types, AC2 rubric total, AC3 due rules, AC4 publish)
        var adminToken = await CreateUserAndGetTokenAsync(Roles.ProgramAdministrator);
        var trackId = (await (await PostAsync("/api/tracks", new
        {
            name = "Assignment Builder",
            key = $"assignment-builder-{Guid.NewGuid():N}",
        }, adminToken)).Content.ReadFromJsonAsync<IdDto>())!.Id;
        var moduleId = (await (await PostAsync("/api/modules", new
        {
            trackId,
            name = "Service Project",
        }, adminToken)).Content.ReadFromJsonAsync<IdDto>())!.Id;
        Guid cohortId;
        var youthIds = new List<Guid>();
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var cohort = new Cohort
            {
                Name = "Assignment Cohort",
                TrackId = trackId,
                TermStart = DateTime.UtcNow.Date,
                TermEnd = DateTime.UtcNow.Date.AddMonths(3),
            };
            cohortId = cohort.Id;
            db.Cohorts.Add(cohort);
            for (var index = 1; index <= 2; index++)
            {
                var youth = new User
                {
                    Email = $"assignment.youth.{index}.{Guid.NewGuid():N}@example.com",
                    DisplayName = $"Assignment Youth {index}",
                    Role = Roles.Youth,
                    Status = UserStatus.Active,
                    CohortId = cohort.Id,
                    PasswordHash = "not-used",
                };
                youthIds.Add(youth.Id);
                db.Users.Add(youth);
            }
            await db.SaveChangesAsync();
        }

        var invalid = await PostAsync("/api/assignments", new
        {
            moduleId,
            title = "Invalid assignment",
            taskDescription = "This must be rejected.",
            submissionTypes = new[] { "executable" },
            gracePeriodDays = -1,
            allowLate = true,
            requireMentorApproval = true,
            rubric = new[] { new { description = "Invalid points", points = -5 } },
        }, adminToken);
        Assert.That(invalid.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var dueDate = DateTime.UtcNow.Date.AddDays(14);
        var created = await PostAsync("/api/assignments", new
        {
            moduleId,
            title = "Community service plan",
            taskDescription = "Plan one practical act of service.",
            submissionTypes = new[] { "text", "link" },
            dueDate,
            gracePeriodDays = 2,
            allowLate = true,
            requireMentorApproval = true,
            rubric = new[]
            {
                new { description = "Practical detail", points = 10 },
                new { description = "Reflection", points = 5 },
            },
        }, adminToken);
        Assert.That(created.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var assignmentId = (await created.Content.ReadFromJsonAsync<IdDto>())!.Id;

        var detailResponse = await GetAsync($"/api/assignments/{assignmentId}", adminToken);
        Assert.That(detailResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var detail = await detailResponse.Content.ReadFromJsonAsync<AssignmentDetailDto>();
        Assert.Multiple(() =>
        {
            Assert.That(detail!.Status, Is.EqualTo("Draft"));
            Assert.That(detail.SubmissionTypes, Is.EqualTo(new[] { "text", "link" }));
            Assert.That(detail.TotalPoints, Is.EqualTo(15));
            Assert.That(detail.GracePeriodDays, Is.EqualTo(2));
            Assert.That(detail.Rubric.Select(item => item.Points), Is.EqualTo(new[] { 10, 5 }));
        });

        var publishedDueDate = dueDate.AddDays(2);
        var update = await PutAsync($"/api/assignments/{assignmentId}", new
        {
            moduleId,
            lessonId = (Guid?)null,
            title = "Revised community service plan",
            taskDescription = "Plan and document one practical act of service.",
            submissionTypes = new[] { "text", "photo" },
            dueDate = publishedDueDate,
            gracePeriodDays = 3,
            allowLate = false,
            requireMentorApproval = true,
            rubric = new[] { new { description = "Complete plan", points = 20 } },
        }, adminToken);
        Assert.That(update.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        var updatedDetail = await (await GetAsync($"/api/assignments/{assignmentId}", adminToken))
            .Content.ReadFromJsonAsync<AssignmentDetailDto>();
        Assert.Multiple(() =>
        {
            Assert.That(updatedDetail!.SubmissionTypes, Is.EqualTo(new[] { "text", "photo" }));
            Assert.That(updatedDetail.TotalPoints, Is.EqualTo(20));
            Assert.That(updatedDetail.GracePeriodDays, Is.EqualTo(3));
            Assert.That(updatedDetail.Rubric.Single().Description, Is.EqualTo("Complete plan"));
        });

        var publish = await PostAsync(
            $"/api/assignments/{assignmentId}/publish",
            new { status = (int)PublishStatus.Published },
            adminToken);
        Assert.That(publish.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        var publishAgain = await PostAsync(
            $"/api/assignments/{assignmentId}/publish",
            new { status = (int)PublishStatus.Published },
            adminToken);
        Assert.That(publishAgain.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        using var verifyScope = Factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var work = await verifyDb.AssignedWork
            .Where(item => item.ItemId == assignmentId)
            .OrderBy(item => item.YouthUserId)
            .ToListAsync();
        Assert.Multiple(() =>
        {
            Assert.That(work.Select(item => item.YouthUserId), Is.EquivalentTo(youthIds));
            Assert.That(work.All(item => item.ItemType == "Project"), Is.True);
            Assert.That(work.All(item => item.CohortId == cohortId && item.DueDate == publishedDueDate), Is.True);
            Assert.That(verifyDb.AuditEntries.Count(item =>
                item.Action == "AssignmentPublishStateChanged" && item.TargetId == assignmentId),
                Is.EqualTo(2));
        });
    }

    private sealed record CompleteQuizDetailDto(
        Guid Id,
        int PassMark,
        int AttemptsAllowed,
        string FeedbackMode,
        IReadOnlyList<CompleteQuestionDto> Questions);
    private sealed record CompleteQuestionDto(
        Guid Id,
        QuestionType Type,
        int Order,
        IReadOnlyList<int> CorrectIndexes,
        IReadOnlyList<string> CorrectAnswers);
    private sealed record AssignmentDetailDto(
        Guid Id,
        string Status,
        IReadOnlyList<string> SubmissionTypes,
        int GracePeriodDays,
        int TotalPoints,
        IReadOnlyList<RubricDetailDto> Rubric);
    private sealed record RubricDetailDto(Guid Id, string Description, int Points, int Order);
}
