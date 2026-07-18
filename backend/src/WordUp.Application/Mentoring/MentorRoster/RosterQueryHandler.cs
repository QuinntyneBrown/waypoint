using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;
using WordUp.Application.Mentoring.Flags;

namespace WordUp.Application.Mentoring.MentorRoster;

public class RosterQueryHandler : IRequestHandler<RosterQuery, IReadOnlyList<RosterYouth>>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly AtRiskEvaluator _atRiskEvaluator;

    public RosterQueryHandler(
        IAppDbContext db,
        IClock clock,
        ICurrentUser currentUser,
        AtRiskEvaluator atRiskEvaluator)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _atRiskEvaluator = atRiskEvaluator;
    }

    public async Task<IReadOnlyList<RosterYouth>> Handle(RosterQuery request, CancellationToken cancellationToken)
    {
        var mentorId = _currentUser.UserId ?? throw new ForbiddenException();

        // Only the mentor's assigned youth (L2-AUTH-010).
        var youth = await _db.Users
            .Where(u => u.MentorId == mentorId && u.Role == Roles.Youth)
            .Select(u => new { u.Id, u.DisplayName, u.CohortId, u.CreatedAt, u.LastSignInAt })
            .ToListAsync(cancellationToken);

        var youthIds = youth.Select(y => y.Id).ToList();
        await _atRiskEvaluator.EvaluateAsync(youthIds, cancellationToken: cancellationToken);
        var cohortIds = youth.Where(item => item.CohortId != null).Select(item => item.CohortId!.Value).Distinct().ToList();
        var cohortNames = await _db.Cohorts
            .Where(cohort => cohortIds.Contains(cohort.Id))
            .ToDictionaryAsync(cohort => cohort.Id, cohort => cohort.Name, cancellationToken);

        var atRiskIds = await _db.Flags
            .Where(f => youthIds.Contains(f.YouthUserId) && f.ResolvedAt == null)
            .Select(f => f.YouthUserId)
            .Distinct()
            .ToListAsync(cancellationToken);
        var atRisk = atRiskIds.ToHashSet();
        var progressRows = await _db.LessonProgress
            .Where(item => youthIds.Contains(item.YouthUserId))
            .Select(item => new { item.YouthUserId, item.Status })
            .ToListAsync(cancellationToken);

        var newThreshold = _clock.UtcNow.AddDays(-30);
        var filter = string.IsNullOrWhiteSpace(request.Filter) ? "all" : request.Filter.Trim().ToLowerInvariant();

        var result = youth
            .Where(y => filter switch
            {
                "at-risk" => atRisk.Contains(y.Id),
                "on-track" => !atRisk.Contains(y.Id),
                "new" => y.CreatedAt >= newThreshold,
                _ => true,
            })
            .Where(y => string.IsNullOrWhiteSpace(request.Search)
                || y.DisplayName.Contains(request.Search.Trim(), StringComparison.OrdinalIgnoreCase))
            .OrderBy(y => y.DisplayName)
            .Select(y =>
            {
                var youthProgress = progressRows.Where(item => item.YouthUserId == y.Id).ToList();
                var progress = youthProgress.Count == 0
                    ? 0
                    : (double)youthProgress.Count(item => item.Status == ProgressStatus.Completed) / youthProgress.Count;
                var isAtRisk = atRisk.Contains(y.Id);
                return new RosterYouth(
                    y.Id,
                    y.DisplayName,
                    isAtRisk ? "At risk" : "On track",
                    isAtRisk,
                    y.CreatedAt >= newThreshold,
                    progress,
                    y.CohortId,
                    y.CohortId is { } cohortId && cohortNames.TryGetValue(cohortId, out var cohortName)
                        ? cohortName
                        : null,
                    y.LastSignInAt ?? y.CreatedAt);
            })
            .ToList();

        return result;
    }
}
