using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using WordUp.Infrastructure;

namespace WordUp.IntegrationTests.Learning;

// Acceptance Test
// Traces to: L2-LEARN-004 (youth quiz taking), L2-LEARN-005 (result view)
[TestFixture]
[Property("Req", "L2-LEARN-004")]
[Property("Req", "L2-LEARN-005")]
public sealed class QuizTakingTests : IntegrationTestBase
{
    private async Task<HttpResponseMessage> PostAsync(string url, object? body, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

    /// <summary>Seeds a quiz (PassMark 50, AttemptsAllowed 2) with two MultipleChoice questions whose
    /// correct options are index 0 and index 1 respectively.</summary>
    private (Guid QuizId, Guid Q1, Guid Q2) SeedQuiz()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var quiz = new Quiz { PassMark = 50, AttemptsAllowed = 2 };
        db.Quizzes.Add(quiz);

        var q1 = new QuizQuestion
        {
            QuizId = quiz.Id,
            Text = "What is the greatest commandment?",
            Type = QuestionType.MultipleChoice,
            Order = 0,
            OptionsJson = JsonSerializer.Serialize(new[] { "Love God", "Love money" }),
            CorrectAnswersJson = JsonSerializer.Serialize(new[] { 0 }),
        };
        var q2 = new QuizQuestion
        {
            QuizId = quiz.Id,
            Text = "Who is the second?",
            Type = QuestionType.MultipleChoice,
            Order = 1,
            OptionsJson = JsonSerializer.Serialize(new[] { "Yourself", "Your neighbour" }),
            CorrectAnswersJson = JsonSerializer.Serialize(new[] { 1 }),
        };
        db.QuizQuestions.AddRange(q1, q2);
        db.SaveChanges();

        return (quiz.Id, q1.Id, q2.Id);
    }

    private sealed record AttemptViewDto(Guid AttemptId, int AttemptNumber, int QuestionCount, int AttemptsAllowed, int PassMark, List<QuestionDtoModel> Questions);

    private sealed record QuestionDtoModel(Guid Id, string Text, string Type, List<string> Options);

    private sealed record ResultDto(int? Score, bool? Passed, string Status);

    private async Task<AttemptViewDto> StartAsync(Guid quizId, string token)
    {
        var response = await PostAsync($"/api/me/quizzes/{quizId}/attempts", null, token);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        return (await response.Content.ReadFromJsonAsync<AttemptViewDto>())!;
    }

    [Test]
    [Property("Req", "L2-LEARN-004")]
    public async Task Starting_an_attempt_returns_questions_without_correct_answers_as_attempt_one()
    {
        // Traces to: L2-LEARN-004
        var (quizId, _, _) = SeedQuiz();
        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);

        var response = await PostAsync($"/api/me/quizzes/{quizId}/attempts", null, youthToken);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var raw = await response.Content.ReadAsStringAsync();
        Assert.That(raw, Does.Not.Contain("Correct").IgnoreCase, "correct answers must never be sent to the youth");

        var view = JsonSerializer.Deserialize<AttemptViewDto>(raw, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        Assert.That(view.AttemptNumber, Is.EqualTo(1));
        Assert.That(view.QuestionCount, Is.EqualTo(2));
        Assert.That(view.Questions, Has.Count.EqualTo(2));
        Assert.That(view.Questions[0].Options, Is.Not.Empty);
    }

    [Test]
    [Property("Req", "L2-LEARN-004")]
    [Property("Req", "L2-LEARN-005")]
    public async Task Submitting_correct_answers_is_pending_approval_and_hides_the_score()
    {
        // Traces to: L2-LEARN-004, L2-LEARN-005
        var (quizId, q1, q2) = SeedQuiz();
        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);

        var attempt = await StartAsync(quizId, youthToken);
        var answers = JsonSerializer.Serialize(new Dictionary<string, int[]>
        {
            [q1.ToString()] = new[] { 0 },
            [q2.ToString()] = new[] { 1 },
        });

        var submit = await PostAsync($"/api/me/attempts/{attempt.AttemptId}/submit", new { answersJson = answers }, youthToken);
        Assert.That(submit.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var result = await submit.Content.ReadFromJsonAsync<ResultDto>();
        Assert.That(result!.Status, Is.EqualTo("PendingApproval"));
        Assert.That(result.Score, Is.Null, "youth cannot see the score before mentor approval");
    }

    [Test]
    [Property("Req", "L2-LEARN-004")]
    public async Task Re_submitting_the_same_attempt_conflicts_because_it_is_immutable()
    {
        // Traces to: L2-LEARN-004
        var (quizId, q1, q2) = SeedQuiz();
        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);

        var attempt = await StartAsync(quizId, youthToken);
        var answers = JsonSerializer.Serialize(new Dictionary<string, int[]>
        {
            [q1.ToString()] = new[] { 0 },
            [q2.ToString()] = new[] { 1 },
        });

        var first = await PostAsync($"/api/me/attempts/{attempt.AttemptId}/submit", new { answersJson = answers }, youthToken);
        Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var second = await PostAsync($"/api/me/attempts/{attempt.AttemptId}/submit", new { answersJson = answers }, youthToken);
        Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    [Property("Req", "L2-LEARN-004")]
    public async Task Starting_after_exhausting_the_attempt_limit_conflicts()
    {
        // Traces to: L2-LEARN-004
        var (quizId, q1, q2) = SeedQuiz();
        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);
        var answers = JsonSerializer.Serialize(new Dictionary<string, int[]>
        {
            [q1.ToString()] = new[] { 0 },
            [q2.ToString()] = new[] { 1 },
        });

        // Consume both allowed attempts.
        for (var i = 0; i < 2; i++)
        {
            var attempt = await StartAsync(quizId, youthToken);
            var submit = await PostAsync($"/api/me/attempts/{attempt.AttemptId}/submit", new { answersJson = answers }, youthToken);
            Assert.That(submit.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        var third = await PostAsync($"/api/me/quizzes/{quizId}/attempts", null, youthToken);
        Assert.That(third.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }
}
