using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Engagement.Notifications;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Mentoring.QuizApproval;

public class ApproveAllQuizAttemptsCommandHandler : IRequestHandler<ApproveAllQuizAttemptsCommand, int>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;
    private readonly NotificationDeliveryService _notifications;

    public ApproveAllQuizAttemptsCommandHandler(
        IAppDbContext db,
        IClock clock,
        IAuditLogger audit,
        ICurrentUser currentUser,
        NotificationDeliveryService notifications)
    {
        _db = db;
        _clock = clock;
        _audit = audit;
        _currentUser = currentUser;
        _notifications = notifications;
    }

    public async Task<int> Handle(ApproveAllQuizAttemptsCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId
            ?? throw new ForbiddenException("No current user.");
        var isAdmin = _currentUser.Role == Roles.ProgramAdministrator;

        // Scope to the mentor's assigned youth (an administrator may approve for everyone).
        var youthQuery = _db.Users.Where(u => u.Role == Roles.Youth);
        if (!isAdmin)
        {
            youthQuery = youthQuery.Where(u => u.MentorId == currentUserId);
        }
        var youthIds = await youthQuery.Select(u => u.Id).ToListAsync(cancellationToken);

        // Pending = submitted but not yet approved.
        var pending = await _db.QuizAttempts
            .Where(a => a.IsSubmitted
                        && a.ApprovedAt == null
                        && youthIds.Contains(a.YouthUserId))
            .ToListAsync(cancellationToken);

        if (pending.Count == 0)
        {
            return 0;
        }

        var now = _clock.UtcNow;

        // Pre-load the quizzes involved so we can evaluate pass marks without N round-trips.
        var quizIds = pending.Select(a => a.QuizId).Distinct().ToList();
        var quizzes = await _db.Quizzes
            .Where(q => quizIds.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id, cancellationToken);

        var badgeNotifications = new List<NotificationDeliveryPlan>();
        foreach (var attempt in pending)
        {
            var approvedScore = attempt.AutoScore;
            attempt.ApprovedScore = approvedScore;
            attempt.ApprovedByUserId = currentUserId;
            attempt.ApprovedAt = now;

            var passed = quizzes.TryGetValue(attempt.QuizId, out var quiz)
                ? approvedScore >= quiz.PassMark
                : attempt.Passed;

            if (passed)
            {
                badgeNotifications.AddRange(await QuizBadgeAwarder.AwardForPassedQuizAsync(
                    _db, _notifications, attempt.QuizId, attempt.YouthUserId, currentUserId, now, cancellationToken));
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        await QuizBadgeAwarder.DispatchNotificationsAsync(
            _notifications,
            badgeNotifications,
            cancellationToken);

        foreach (var attempt in pending)
        {
            await _audit.LogAsync(
                "QuizApproved",
                currentUserId,
                "QuizAttempt",
                attempt.Id,
                details: $"auto={attempt.AutoScore};bulk=true",
                cancellationToken: cancellationToken);
        }

        await _audit.LogAsync(
            "QuizApprovedBulk", currentUserId, "QuizAttempt", null, details: $"count={pending.Count}");

        return pending.Count;
    }
}
