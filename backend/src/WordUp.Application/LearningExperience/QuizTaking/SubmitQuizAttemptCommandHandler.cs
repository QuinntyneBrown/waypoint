using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Engagement.Notifications;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.LearningExperience.QuizTaking;

public class SubmitQuizAttemptCommandHandler : IRequestHandler<SubmitQuizAttemptCommand, QuizResultView>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly NotificationDeliveryService _notifications;

    public SubmitQuizAttemptCommandHandler(
        IAppDbContext db,
        ICurrentUser currentUser,
        IClock clock,
        IAuditLogger audit,
        NotificationDeliveryService notifications)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
        _audit = audit;
        _notifications = notifications;
    }

    public async Task<QuizResultView> Handle(SubmitQuizAttemptCommand request, CancellationToken cancellationToken)
    {
        var attempt = await _db.QuizAttempts.FirstOrDefaultAsync(a => a.Id == request.AttemptId, cancellationToken)
            ?? throw new NotFoundException("Attempt not found.");

        if (attempt.YouthUserId != _currentUser.UserId)
        {
            throw new ForbiddenException("This attempt does not belong to you.");
        }

        if (attempt.IsSubmitted)
        {
            throw new ConflictException("This attempt has already been submitted.");
        }

        var quiz = await _db.Quizzes.FirstOrDefaultAsync(q => q.Id == attempt.QuizId, cancellationToken)
            ?? throw new NotFoundException("Quiz not found.");

        var questions = await _db.QuizQuestions
            .Where(q => q.QuizId == quiz.Id)
            .ToListAsync(cancellationToken);

        // Score server-side against the stored correct answers (never trust the client).
        var answers = QuizAnswerScoring.ParseAnswerValues(request.AnswersJson);
        var correct = 0;
        foreach (var question in questions)
        {
            answers.TryGetValue(question.Id, out var given);
            if (QuizAnswerScoring.IsCorrect(question, given))
            {
                correct++;
            }
        }

        var total = questions.Count;
        var autoScore = total == 0 ? 0 : (int)Math.Round(100.0 * correct / total, MidpointRounding.AwayFromZero);

        attempt.AnswersJson = request.AnswersJson;
        attempt.AutoScore = autoScore;
        attempt.Passed = autoScore >= quiz.PassMark;
        attempt.IsSubmitted = true;
        var now = _clock.UtcNow;
        attempt.SubmittedAt = now;
        // ApprovedScore intentionally left null: a mentor must approve before the youth sees a score.

        NotificationDeliveryPlan? approvalPlan = null;
        var submitter = await _db.Users.AsNoTracking()
            .Where(item => item.Id == attempt.YouthUserId)
            .Select(item => new { item.DisplayName, item.MentorId })
            .SingleAsync(cancellationToken);
        if (submitter.MentorId is { } mentorId)
        {
            approvalPlan = await _notifications.PrepareAsync(
                mentorId,
                "approval-needed",
                "Quiz ready for review",
                "A participant submitted a quiz attempt.",
                $"/mentor/reviews/quizzes/{attempt.Id}",
                now,
                cancellationToken,
                subjectUserId: attempt.YouthUserId);
        }

        await _db.SaveChangesAsync(cancellationToken);
        if (approvalPlan is not null)
        {
            await _notifications.DispatchAsync(approvalPlan, cancellationToken);
        }
        await _audit.LogAsync("QuizAttemptSubmitted", _currentUser.UserId, "QuizAttempt", attempt.Id);

        // The youth does not see the score until approved (L2-LEARN-005 AC3).
        var submittedCount = await _db.QuizAttempts.CountAsync(
            item => item.QuizId == quiz.Id
                && item.YouthUserId == attempt.YouthUserId
                && item.IsSubmitted,
            cancellationToken);
        return new QuizResultView(
            attempt.Id,
            quiz.Id,
            attempt.AttemptNumber,
            null,
            quiz.PassMark,
            null,
            "PendingApproval",
            quiz.FeedbackMode,
            Array.Empty<QuizQuestionReview>(),
            null,
            Math.Max(0, quiz.AttemptsAllowed - submittedCount),
            false,
            null);
    }
}
