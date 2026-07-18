using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.LearningExperience.QuizTaking;

public class StartQuizAttemptQueryHandler : IRequestHandler<StartQuizAttemptQuery, QuizAttemptView>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditLogger _audit;

    public StartQuizAttemptQueryHandler(IAppDbContext db, ICurrentUser currentUser, IAuditLogger audit)
    {
        _db = db;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<QuizAttemptView> Handle(StartQuizAttemptQuery request, CancellationToken cancellationToken)
    {
        var youthId = _currentUser.UserId ?? throw new ForbiddenException("Not signed in.");

        var accessible = await QuizAccessGuard.EnsureAccessibleAsync(
            _db, request.QuizId, youthId, cancellationToken);
        var quiz = accessible.Quiz;

        var attempts = await _db.QuizAttempts
            .Where(a => a.QuizId == quiz.Id && a.YouthUserId == youthId)
            .ToListAsync(cancellationToken);

        var submittedCount = attempts.Count(a => a.IsSubmitted);
        var open = attempts.FirstOrDefault(a => !a.IsSubmitted);

        QuizAttempt attempt;
        if (open is not null)
        {
            // Resume the in-progress attempt (L2-LEARN-004 AC2).
            attempt = open;
        }
        else
        {
            if (submittedCount >= quiz.AttemptsAllowed)
            {
                throw new ConflictException("No attempts remaining.");
            }

            attempt = new QuizAttempt
            {
                YouthUserId = youthId,
                QuizId = quiz.Id,
                AttemptNumber = submittedCount + 1,
            };
            _db.QuizAttempts.Add(attempt);
            await _db.SaveChangesAsync(cancellationToken);
            await _audit.LogAsync("QuizAttemptStarted", youthId, "QuizAttempt", attempt.Id);
        }

        var questions = await _db.QuizQuestions
            .Where(q => q.QuizId == quiz.Id)
            .OrderBy(q => q.Order)
            .ToListAsync(cancellationToken);

        var questionDtos = questions
            .Select(q => new QuestionDto(q.Id, q.Text, q.Type.ToString(), QuizAnswerScoring.ParseOptions(q.OptionsJson)))
            .ToList();
        var badge = await _db.Badges.AsNoTracking()
            .Where(item => item.AutoRule == "quiz-passed"
                && item.TargetRefId == quiz.Id
                && item.ArchivedAt == null)
            .Select(item => new QuizBadgeView(item.Id, item.Name, item.Icon, item.Description))
            .FirstOrDefaultAsync(cancellationToken);
        var currentQuestionIndex = questions.Count == 0
            ? 0
            : Math.Clamp(attempt.CurrentQuestionIndex, 0, questions.Count - 1);

        return new QuizAttemptView(
            attempt.Id,
            quiz.Id,
            accessible.Title,
            attempt.AttemptNumber,
            questions.Count,
            Math.Max(1, questions.Count * 2),
            quiz.AttemptsAllowed,
            quiz.PassMark,
            badge,
            currentQuestionIndex,
            attempt.AnswersJson,
            questionDtos);
    }
}
