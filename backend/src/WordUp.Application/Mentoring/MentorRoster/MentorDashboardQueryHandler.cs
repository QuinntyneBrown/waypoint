using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;
using WordUp.Application.Mentoring.Flags;

namespace WordUp.Application.Mentoring.MentorRoster;

public class MentorDashboardQueryHandler : IRequestHandler<MentorDashboardQuery, MentorDashboard>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly AtRiskEvaluator _atRiskEvaluator;

    public MentorDashboardQueryHandler(
        IAppDbContext db,
        ICurrentUser currentUser,
        IClock clock,
        AtRiskEvaluator atRiskEvaluator)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
        _atRiskEvaluator = atRiskEvaluator;
    }

    public async Task<MentorDashboard> Handle(MentorDashboardQuery request, CancellationToken cancellationToken)
    {
        var mentorId = _currentUser.UserId ?? throw new ForbiddenException();

        // Only the current mentor's assigned youth (L2-MENTOR-001 AC1 / L2-AUTH-010).
        var youthIds = await _db.Users
            .Where(u => u.MentorId == mentorId && u.Role == Roles.Youth)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        var youthCount = youthIds.Count;
        await _atRiskEvaluator.EvaluateAsync(youthIds, cancellationToken: cancellationToken);

        // Pending reviews = submissions awaiting review + quiz attempts submitted-but-not-approved.
        var pendingSubmissions = await _db.Submissions
            .Where(s => youthIds.Contains(s.YouthUserId)
                && (s.Status == SubmissionStatus.Submitted || s.Status == SubmissionStatus.UnderReview))
            .Select(s => s.AssignmentId)
            .ToListAsync(cancellationToken);

        var pendingAttempts = await _db.QuizAttempts
            .CountAsync(a => youthIds.Contains(a.YouthUserId) && a.IsSubmitted && a.ApprovedScore == null,
                cancellationToken);

        var pendingReviews = pendingSubmissions.Count + pendingAttempts;
        var pendingAssignmentIds = pendingSubmissions.Distinct().ToList();
        var overdueAssignmentIds = await _db.Assignments
            .Where(assignment => pendingAssignmentIds.Contains(assignment.Id)
                && assignment.DueDate != null
                && assignment.DueDate < _clock.UtcNow)
            .Select(assignment => assignment.Id)
            .ToListAsync(cancellationToken);
        var overdueAssignmentIdSet = overdueAssignmentIds.ToHashSet();
        var overdueReviews = pendingSubmissions.Count(overdueAssignmentIdSet.Contains);

        // At-risk = distinct youth with an active flag.
        var atRiskCount = await _db.Flags
            .Where(f => youthIds.Contains(f.YouthUserId) && f.ResolvedAt == null)
            .Select(f => f.YouthUserId)
            .Distinct()
            .CountAsync(cancellationToken);

        // Average progress = mean, across the mentor's youth, of their fraction of completed lessons.
        var averageProgress = 0.0;
        if (youthCount > 0)
        {
            var progress = await _db.LessonProgress
                .Where(lp => youthIds.Contains(lp.YouthUserId))
                .Select(lp => new { lp.YouthUserId, lp.Status })
                .ToListAsync(cancellationToken);

            averageProgress = youthIds
                .Select(id =>
                {
                    var rows = progress.Where(p => p.YouthUserId == id).ToList();
                    return rows.Count == 0
                        ? 0.0
                        : (double)rows.Count(r => r.Status == ProgressStatus.Completed) / rows.Count;
                })
                .Average();
        }

        return new MentorDashboard(youthCount, pendingReviews, overdueReviews, averageProgress, atRiskCount);
    }
}
