using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;
using WordUp.Domain;

namespace WordUp.Application.MentorRoster;

public class RosterQueryHandler : IRequestHandler<RosterQuery, IReadOnlyList<RosterYouth>>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;

    public RosterQueryHandler(IAppDbContext db, IClock clock, ICurrentUser currentUser)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<RosterYouth>> Handle(RosterQuery request, CancellationToken cancellationToken)
    {
        var mentorId = _currentUser.UserId ?? throw new ForbiddenException();

        // Only the mentor's assigned youth (L2-AUTH-010).
        var youth = await _db.Users
            .Where(u => u.MentorId == mentorId && u.Role == Roles.Youth)
            .Select(u => new { u.Id, u.DisplayName, u.Status, u.CreatedAt })
            .ToListAsync(cancellationToken);

        var youthIds = youth.Select(y => y.Id).ToList();

        var atRiskIds = await _db.Flags
            .Where(f => youthIds.Contains(f.YouthUserId) && f.ResolvedAt == null)
            .Select(f => f.YouthUserId)
            .Distinct()
            .ToListAsync(cancellationToken);
        var atRisk = atRiskIds.ToHashSet();

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
            .Select(y => new RosterYouth(y.Id, y.DisplayName, y.Status.ToString(), atRisk.Contains(y.Id)))
            .ToList();

        return result;
    }
}
