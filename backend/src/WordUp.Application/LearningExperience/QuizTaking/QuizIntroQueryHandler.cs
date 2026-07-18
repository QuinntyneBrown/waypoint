using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.LearningExperience.QuizTaking;

public sealed class QuizIntroQueryHandler : IRequestHandler<QuizIntroQuery, QuizIntroView>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public QuizIntroQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<QuizIntroView> Handle(QuizIntroQuery request, CancellationToken cancellationToken)
    {
        var youthId = _currentUser.UserId ?? throw new ForbiddenException("Not signed in.");
        var accessible = await QuizAccessGuard.EnsureAccessibleAsync(
            _db, request.QuizId, youthId, cancellationToken);
        var questionCount = await _db.QuizQuestions
            .CountAsync(question => question.QuizId == request.QuizId, cancellationToken);
        var attempts = await _db.QuizAttempts
            .Where(attempt => attempt.QuizId == request.QuizId && attempt.YouthUserId == youthId)
            .ToListAsync(cancellationToken);
        var open = attempts.FirstOrDefault(attempt => !attempt.IsSubmitted);
        var submittedCount = attempts.Count(attempt => attempt.IsSubmitted);
        var attemptsRemaining = Math.Max(0, accessible.Quiz.AttemptsAllowed - submittedCount);
        var badge = await _db.Badges.AsNoTracking()
            .Where(item => item.AutoRule == "quiz-passed"
                && item.TargetRefId == accessible.Quiz.Id
                && item.ArchivedAt == null)
            .Select(item => new QuizBadgeView(item.Id, item.Name, item.Icon, item.Description))
            .FirstOrDefaultAsync(cancellationToken);

        return new QuizIntroView(
            accessible.Quiz.Id,
            accessible.Title,
            questionCount,
            Math.Max(1, questionCount * 2),
            accessible.Quiz.AttemptsAllowed,
            accessible.Quiz.PassMark,
            badge,
            open?.AttemptNumber ?? Math.Min(submittedCount + 1, accessible.Quiz.AttemptsAllowed),
            attemptsRemaining,
            open is not null,
            attemptsRemaining == 0 && open is null);
    }
}
