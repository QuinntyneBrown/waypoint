using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Infrastructure;

namespace Waypoint.IntegrationTests.Learning;

// Acceptance Test
// Traces to: L2-LEARN-004 (youth quiz taking), L2-LEARN-005 (result view)
[TestFixture]
[Property("Req", "L2-LEARN-004")]
[Property("Req", "L2-LEARN-005")]
public sealed class QuizTakingTests : IntegrationTestBase
{
    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string url,
        object? body,
        string token)
    {
        var request = new HttpRequestMessage(method, url);
        if (body is not null) request.Content = JsonContent.Create(body);
        Authorize(request, token);
        return await Client.SendAsync(request);
    }

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
    private sealed record SeededQuiz(Guid QuizId, Guid Q1, Guid Q2, Guid BadgeId, string BadgeName);

    private SeededQuiz SeedQuiz(string feedbackMode = "show-correct")
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var quiz = new Quiz { PassMark = 50, AttemptsAllowed = 2, FeedbackMode = feedbackMode };
        db.Quizzes.Add(quiz);
        var badge = new Badge
        {
            Name = $"Quiz badge {Guid.NewGuid():N}",
            Icon = "spark",
            CriteriaKind = BadgeCriteriaKind.Auto,
            AutoRule = "quiz-passed",
            TargetRefId = quiz.Id,
        };
        db.Badges.Add(badge);

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

        return new SeededQuiz(quiz.Id, q1.Id, q2.Id, badge.Id, badge.Name);
    }

    private sealed record BadgeDto(Guid Id, string Name, string Icon, string? Description);

    private sealed record IntroDto(
        Guid QuizId,
        string Title,
        int QuestionCount,
        int EstimatedMinutes,
        int AttemptsAllowed,
        int PassMark,
        BadgeDto? BadgeReward,
        int CurrentAttemptNumber,
        int AttemptsRemaining,
        bool HasInProgressAttempt,
        bool AttemptsExhausted);

    private sealed record AttemptViewDto(
        Guid AttemptId,
        Guid QuizId,
        string Title,
        int AttemptNumber,
        int QuestionCount,
        int EstimatedMinutes,
        int AttemptsAllowed,
        int PassMark,
        BadgeDto? BadgeReward,
        int CurrentQuestionIndex,
        string? AnswersJson,
        List<QuestionDtoModel> Questions);

    private sealed record QuestionDtoModel(Guid Id, string Text, string Type, List<string> Options);

    private sealed record ReviewDto(
        Guid QuestionId,
        string Text,
        List<string> Options,
        List<int> SelectedIndexes,
        List<int> CorrectIndexes,
        bool Correct);

    private sealed record ResultDto(
        Guid AttemptId,
        Guid QuizId,
        int AttemptNumber,
        int? Score,
        int PassMark,
        bool? Passed,
        string Status,
        string FeedbackMode,
        List<ReviewDto> Review,
        BadgeDto? EarnedBadge,
        int AttemptsRemaining,
        bool CanRetake,
        int? BestPassingScore);

    private async Task<AttemptViewDto> StartAsync(Guid quizId, string token)
    {
        var response = await PostAsync($"/api/me/quizzes/{quizId}/attempts", null, token);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        return (await response.Content.ReadFromJsonAsync<AttemptViewDto>())!;
    }

    private async Task<(User Mentor, User Youth, string YouthToken)> SeedMentoredYouthAsync()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var mentor = new User
        {
            Email = $"quiz.mentor.{suffix}@example.com",
            DisplayName = "Quiz Mentor",
            Role = Roles.Mentor,
            Status = UserStatus.Active,
        };
        var youth = new User
        {
            Email = $"quiz.youth.{suffix}@example.com",
            DisplayName = "Quiz Youth",
            Role = Roles.Youth,
            Status = UserStatus.Active,
            MentorId = mentor.Id,
        };
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            mentor.PasswordHash = hasher.Hash("Passw0rd!");
            youth.PasswordHash = hasher.Hash("Passw0rd!");
            db.Users.AddRange(mentor, youth);
            db.NotificationPreferences.Add(new NotificationPreference
            {
                UserId = mentor.Id,
                EventKey = "approval-needed",
                InApp = false,
                Email = true,
            });
            await db.SaveChangesAsync();
        }

        var signIn = await Client.PostAsJsonAsync(
            "/api/auth/sign-in",
            new { email = youth.Email, password = "Passw0rd!" });
        signIn.EnsureSuccessStatusCode();
        var auth = await signIn.Content.ReadFromJsonAsync<Waypoint.Application.Authentication.Auth.AuthResult>();
        return (mentor, youth, auth!.AccessToken);
    }

    [Test]
    [Property("Req", "L2-LEARN-004")]
    public async Task Quiz_intro_and_in_progress_attempt_restore_saved_answers_and_position()
    {
        var seed = SeedQuiz();
        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);

        var introResponse = await SendAsync(
            HttpMethod.Get,
            $"/api/me/quizzes/{seed.QuizId}",
            null,
            youthToken);
        Assert.That(introResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var intro = await introResponse.Content.ReadFromJsonAsync<IntroDto>();
        Assert.Multiple(() =>
        {
            Assert.That(intro!.QuestionCount, Is.EqualTo(2));
            Assert.That(intro.EstimatedMinutes, Is.GreaterThan(0));
            Assert.That(intro.AttemptsAllowed, Is.EqualTo(2));
            Assert.That(intro.PassMark, Is.EqualTo(50));
            Assert.That(intro.BadgeReward!.Name, Is.EqualTo(seed.BadgeName));
            Assert.That(intro.CurrentAttemptNumber, Is.EqualTo(1));
            Assert.That(intro.HasInProgressAttempt, Is.False);
        });

        var attempt = await StartAsync(seed.QuizId, youthToken);
        var answers = JsonSerializer.Serialize(new Dictionary<string, int[]>
        {
            [seed.Q1.ToString()] = new[] { 0 },
        });
        var saved = await SendAsync(
            HttpMethod.Put,
            $"/api/me/attempts/{attempt.AttemptId}/answers",
            new { answersJson = answers, currentQuestionIndex = 1 },
            youthToken);
        Assert.That(saved.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var resumed = await StartAsync(seed.QuizId, youthToken);
        Assert.Multiple(() =>
        {
            Assert.That(resumed.AttemptId, Is.EqualTo(attempt.AttemptId));
            Assert.That(resumed.CurrentQuestionIndex, Is.EqualTo(1));
            Assert.That(resumed.AnswersJson, Is.EqualTo(answers));
        });
    }

    [Test]
    [Property("Req", "L2-LEARN-004")]
    public async Task Starting_an_attempt_returns_questions_without_correct_answers_as_attempt_one()
    {
        // Traces to: L2-LEARN-004
        var seed = SeedQuiz();
        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);

        var response = await PostAsync($"/api/me/quizzes/{seed.QuizId}/attempts", null, youthToken);
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
    [Property("Req", "L2-ENGAGE-008")]
    [Property("Req", "L2-PLATFORM-003")]
    public async Task Submitting_correct_answers_is_pending_approval_and_hides_the_score()
    {
        // Traces to: L2-LEARN-004, L2-LEARN-005, L2-ENGAGE-008, L2-PLATFORM-003 AC2
        var seed = SeedQuiz();
        var (mentor, _, youthToken) = await SeedMentoredYouthAsync();

        var attempt = await StartAsync(seed.QuizId, youthToken);
        var answers = JsonSerializer.Serialize(new Dictionary<string, int[]>
        {
            [seed.Q1.ToString()] = new[] { 0 },
            [seed.Q2.ToString()] = new[] { 1 },
        });

        var submit = await PostAsync($"/api/me/attempts/{attempt.AttemptId}/submit", new { answersJson = answers }, youthToken);
        Assert.That(submit.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var result = await submit.Content.ReadFromJsonAsync<ResultDto>();
        Assert.That(result!.Status, Is.EqualTo("PendingApproval"));
        Assert.That(result.Score, Is.Null, "youth cannot see the score before mentor approval");
        Assert.That(result.Review, Is.Empty, "feedback is also withheld before approval");
        var approvalEmail = Factory.Emails.LastTo(mentor.Email);
        Assert.Multiple(() =>
        {
            Assert.That(approvalEmail, Is.Not.Null, "the assigned mentor receives the quiz review request");
            Assert.That(approvalEmail!.Subject, Is.EqualTo("Quiz ready for review"));
            Assert.That(approvalEmail.Body, Does.Contain($"/mentor/reviews/quizzes/{attempt.AttemptId}"));
            Assert.That(Factory.Changes.Changes.Any(item =>
                item.Audience == "user"
                && item.AudienceId == mentor.Id
                && item.EventName == "notification"
                && JsonSerializer.Serialize(item.Payload).Contains("approval-needed")), Is.False);
        });
        using var notificationScope = Factory.Services.CreateScope();
        var notificationDb = notificationScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.That(notificationDb.Notifications.Any(item =>
            item.UserId == mentor.Id && item.EventKey == "approval-needed"), Is.False);
    }

    [Test]
    [Property("Req", "L2-LEARN-005")]
    public async Task Approved_result_shows_feedback_wrong_selection_and_the_earned_badge()
    {
        var seed = SeedQuiz();
        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);
        var attempt = await StartAsync(seed.QuizId, youthToken);
        var answers = JsonSerializer.Serialize(new Dictionary<string, int[]>
        {
            [seed.Q1.ToString()] = new[] { 1 },
            [seed.Q2.ToString()] = new[] { 1 },
        });
        var submit = await PostAsync(
            $"/api/me/attempts/{attempt.AttemptId}/submit",
            new { answersJson = answers },
            youthToken);
        Assert.That(submit.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var stored = await db.QuizAttempts.FindAsync(attempt.AttemptId);
            stored!.ApprovedScore = 50;
            stored.ApprovedAt = DateTime.UtcNow;
            db.BadgeAwards.Add(new BadgeAward
            {
                BadgeId = seed.BadgeId,
                YouthUserId = stored.YouthUserId,
            });
            await db.SaveChangesAsync();
        }

        var resultResponse = await SendAsync(
            HttpMethod.Get,
            $"/api/me/attempts/{attempt.AttemptId}",
            null,
            youthToken);
        var result = await resultResponse.Content.ReadFromJsonAsync<ResultDto>();
        Assert.Multiple(() =>
        {
            Assert.That(result!.Status, Is.EqualTo("Approved"));
            Assert.That(result.Score, Is.EqualTo(50));
            Assert.That(result.Passed, Is.True);
            Assert.That(result.Review, Has.Count.EqualTo(2));
            Assert.That(result.Review[0].Correct, Is.False);
            Assert.That(result.Review[0].SelectedIndexes, Is.EqualTo(new[] { 1 }));
            Assert.That(result.Review[0].CorrectIndexes, Is.EqualTo(new[] { 0 }));
            Assert.That(result.EarnedBadge!.Name, Is.EqualTo(seed.BadgeName));
            Assert.That(result.AttemptsRemaining, Is.EqualTo(1));
            Assert.That(result.CanRetake, Is.True);
            Assert.That(result.BestPassingScore, Is.EqualTo(50));
        });
    }

    [Test]
    [Property("Req", "L2-LEARN-005")]
    public async Task Score_only_feedback_mode_never_returns_question_review()
    {
        var seed = SeedQuiz("score-only");
        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);
        var attempt = await StartAsync(seed.QuizId, youthToken);
        var answers = JsonSerializer.Serialize(new Dictionary<string, int[]>
        {
            [seed.Q1.ToString()] = new[] { 0 },
            [seed.Q2.ToString()] = new[] { 1 },
        });
        await PostAsync($"/api/me/attempts/{attempt.AttemptId}/submit", new { answersJson = answers }, youthToken);
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var stored = await db.QuizAttempts.FindAsync(attempt.AttemptId);
            stored!.ApprovedScore = 100;
            await db.SaveChangesAsync();
        }

        var resultResponse = await SendAsync(
            HttpMethod.Get,
            $"/api/me/attempts/{attempt.AttemptId}",
            null,
            youthToken);
        var result = await resultResponse.Content.ReadFromJsonAsync<ResultDto>();
        Assert.That(result!.Review, Is.Empty);
    }

    [Test]
    [Property("Req", "L2-LEARN-004")]
    public async Task Re_submitting_the_same_attempt_conflicts_because_it_is_immutable()
    {
        // Traces to: L2-LEARN-004
        var seed = SeedQuiz();
        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);

        var attempt = await StartAsync(seed.QuizId, youthToken);
        var answers = JsonSerializer.Serialize(new Dictionary<string, int[]>
        {
            [seed.Q1.ToString()] = new[] { 0 },
            [seed.Q2.ToString()] = new[] { 1 },
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
        var seed = SeedQuiz();
        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);
        var answers = JsonSerializer.Serialize(new Dictionary<string, int[]>
        {
            [seed.Q1.ToString()] = new[] { 0 },
            [seed.Q2.ToString()] = new[] { 1 },
        });

        // Consume both allowed attempts.
        for (var i = 0; i < 2; i++)
        {
            var attempt = await StartAsync(seed.QuizId, youthToken);
            var submit = await PostAsync($"/api/me/attempts/{attempt.AttemptId}/submit", new { answersJson = answers }, youthToken);
            Assert.That(submit.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        var third = await PostAsync($"/api/me/quizzes/{seed.QuizId}/attempts", null, youthToken);
        Assert.That(third.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    [Property("Req", "L2-CURRIC-003")]
    [Property("Req", "L2-LEARN-004")]
    public async Task Short_answer_scoring_uses_the_authored_text_answers_case_insensitively()
    {
        // Traces to: L2-CURRIC-003 (AC1/AC4), L2-LEARN-004
        Guid quizId;
        Guid questionId;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var quiz = new Quiz { PassMark = 100, AttemptsAllowed = 2 };
            var question = new QuizQuestion
            {
                QuizId = quiz.Id,
                Text = "Complete the phrase: saved by ____.",
                Type = QuestionType.ShortAnswer,
                OptionsJson = "[]",
                CorrectAnswersJson = JsonSerializer.Serialize(new[] { "grace", "God's grace" }),
            };
            db.Quizzes.Add(quiz);
            db.QuizQuestions.Add(question);
            await db.SaveChangesAsync();
            quizId = quiz.Id;
            questionId = question.Id;
        }

        var youthToken = await CreateUserAndGetTokenAsync(Roles.Youth);
        var attempt = await StartAsync(quizId, youthToken);
        Assert.Multiple(() =>
        {
            Assert.That(attempt.Questions.Single().Type, Is.EqualTo("ShortAnswer"));
            Assert.That(attempt.Questions.Single().Options, Is.Empty);
        });
        var answers = JsonSerializer.Serialize(new Dictionary<string, string>
        {
            [questionId.ToString()] = "  GRACE  ",
        });
        var submitted = await PostAsync(
            $"/api/me/attempts/{attempt.AttemptId}/submit",
            new { answersJson = answers },
            youthToken);
        Assert.That(submitted.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var verifyScope = Factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var stored = await verifyDb.QuizAttempts.FindAsync(attempt.AttemptId);
        Assert.Multiple(() =>
        {
            Assert.That(stored!.AutoScore, Is.EqualTo(100));
            Assert.That(stored.Passed, Is.True);
        });
    }
}
