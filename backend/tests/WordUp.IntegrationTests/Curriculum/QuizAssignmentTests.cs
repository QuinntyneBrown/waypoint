using System.Net;
using System.Net.Http.Json;
using WordUp.Domain;

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
}
