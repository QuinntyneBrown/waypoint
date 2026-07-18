using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Mentoring.QuizApproval;

public class ApproveQuizAttemptCommandHandler : IRequestHandler<ApproveQuizAttemptCommand>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public ApproveQuizAttemptCommandHandler(
        IAppDbContext db, IClock clock, IAuditLogger audit, ICurrentUser currentUser)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task Handle(ApproveQuizAttemptCommand request, CancellationToken cancellationToken)
    {
        var attempt = await _db.QuizAttempts
            .FirstOrDefaultAsync(a => a.Id == request.AttemptId, cancellationToken)
            ?? throw new NotFoundException("Quiz attempt not found.");

        var youth = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == attempt.YouthUserId, cancellationToken)
            ?? throw new NotFoundException("Attempt owner not found.");

        // Enforce mentor-assignment (L2-MENTOR-003 AC5 / L2-AUTH-010 no-IDOR).
        if (_currentUser.Role == Roles.Mentor && youth.MentorId != _currentUser.UserId)
        {
            throw new ForbiddenException("This youth is not assigned to you.");
        }

        var now = _clock.UtcNow;
        var approvedScore = request.OverrideScore ?? attempt.AutoScore;

        // Retain AutoScore; record the approved (possibly overridden) score (L2-MENTOR-004 AC4).
        attempt.ApprovedScore = approvedScore;
        attempt.ApprovedByUserId = _currentUser.UserId;
        attempt.ApprovedAt = now;

        // Determine passing against the quiz pass mark; award quiz-passed badges if passed.
        var quiz = await _db.Quizzes
            .FirstOrDefaultAsync(q => q.Id == attempt.QuizId, cancellationToken);
        var passed = quiz is not null ? approvedScore >= quiz.PassMark : attempt.Passed;

        if (passed)
        {
            await QuizBadgeAwarder.AwardForPassedQuizAsync(
                _db, attempt.QuizId, attempt.YouthUserId, _currentUser.UserId, now, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);

        var details = request.OverrideScore is { } overridden
            ? $"auto={attempt.AutoScore};override={overridden}"
            : $"auto={attempt.AutoScore}";
        await _audit.LogAsync("QuizApproved", _currentUser.UserId, "QuizAttempt", attempt.Id, details: details);
    }
}
