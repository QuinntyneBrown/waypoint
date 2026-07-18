using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.LearningExperience.QuizTaking;

public class QuizResultQueryHandler : IRequestHandler<QuizResultQuery, QuizResultView>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public QuizResultQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<QuizResultView> Handle(QuizResultQuery request, CancellationToken cancellationToken)
    {
        var attempt = await _db.QuizAttempts.FirstOrDefaultAsync(a => a.Id == request.AttemptId, cancellationToken)
            ?? throw new NotFoundException("Attempt not found.");

        if (attempt.YouthUserId != _currentUser.UserId)
        {
            throw new ForbiddenException("This attempt does not belong to you.");
        }

        var quiz = await _db.Quizzes.FirstOrDefaultAsync(q => q.Id == attempt.QuizId, cancellationToken)
            ?? throw new NotFoundException("Quiz not found.");
        var submittedCount = await _db.QuizAttempts.CountAsync(
            item => item.QuizId == quiz.Id
                && item.YouthUserId == attempt.YouthUserId
                && item.IsSubmitted,
            cancellationToken);
        var attemptsRemaining = Math.Max(0, quiz.AttemptsAllowed - submittedCount);

        if (!attempt.IsSubmitted || attempt.ApprovedScore is null)
        {
            // Withhold score and answer feedback until a mentor approves (L2-LEARN-005 AC3).
            return new QuizResultView(
                attempt.Id,
                quiz.Id,
                attempt.AttemptNumber,
                null,
                quiz.PassMark,
                null,
                attempt.IsSubmitted ? "PendingApproval" : "InProgress",
                quiz.FeedbackMode,
                Array.Empty<QuizQuestionReview>(),
                null,
                attemptsRemaining,
                false,
                null);
        }

        var approved = attempt.ApprovedScore.Value;
        var answers = QuizAnswerScoring.ParseAnswers(attempt.AnswersJson);
        var revealFeedback = quiz.FeedbackMode == "show-correct"
            || (quiz.FeedbackMode == "after-all-attempts" && attemptsRemaining == 0);
        IReadOnlyList<QuizQuestionReview> review = Array.Empty<QuizQuestionReview>();
        if (revealFeedback)
        {
            var questions = await _db.QuizQuestions.AsNoTracking()
                .Where(question => question.QuizId == quiz.Id)
                .OrderBy(question => question.Order)
                .ToListAsync(cancellationToken);
            review = questions.Select(question =>
            {
                var selected = answers.TryGetValue(question.Id, out var value)
                    ? value.OrderBy(index => index).ToList()
                    : new List<int>();
                var correct = QuizAnswerScoring.ParseIntSet(question.CorrectAnswersJson)
                    .OrderBy(index => index)
                    .ToList();
                return new QuizQuestionReview(
                    question.Id,
                    question.Text,
                    QuizAnswerScoring.ParseOptions(question.OptionsJson),
                    selected,
                    correct,
                    selected.SequenceEqual(correct));
            }).ToList();
        }

        var earnedBadge = await (
            from award in _db.BadgeAwards.AsNoTracking()
            join badge in _db.Badges.AsNoTracking() on award.BadgeId equals badge.Id
            where award.YouthUserId == attempt.YouthUserId
                && badge.AutoRule == "quiz-passed"
                && badge.TargetRefId == quiz.Id
                && badge.ArchivedAt == null
            select new QuizBadgeView(badge.Id, badge.Name, badge.Icon, badge.Description))
            .FirstOrDefaultAsync(cancellationToken);
        var bestPassingScore = await _db.QuizAttempts
            .Where(item => item.QuizId == quiz.Id
                && item.YouthUserId == attempt.YouthUserId
                && item.ApprovedScore != null
                && item.ApprovedScore >= quiz.PassMark)
            .MaxAsync(item => (int?)item.ApprovedScore, cancellationToken);

        return new QuizResultView(
            attempt.Id,
            quiz.Id,
            attempt.AttemptNumber,
            approved,
            quiz.PassMark,
            approved >= quiz.PassMark,
            "Approved",
            quiz.FeedbackMode,
            review,
            earnedBadge,
            attemptsRemaining,
            attemptsRemaining > 0,
            bestPassingScore);
    }
}
