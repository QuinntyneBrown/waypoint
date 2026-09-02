using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Application.Mentoring.CheckIns;

public class CheckInHistoryQueryHandler : IRequestHandler<CheckInHistoryQuery, IReadOnlyList<CheckInSummary>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CheckInHistoryQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<CheckInSummary>> Handle(CheckInHistoryQuery request, CancellationToken cancellationToken)
    {
        // Only the mentor's own check-ins are visible (MENTOR-006 AC3/AC5).
        var query = _db.CheckIns
            .Where(c => c.MentorUserId == _currentUser.UserId);

        if (request.YouthId is { } youthId)
        {
            query = query.Where(c => c.YouthUserId == youthId);
        }

        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            query = query.Where(c => c.Type == request.Type);
        }

        var checkIns = await query
            .OrderByDescending(c => c.Date)
            .ToListAsync(cancellationToken);
        var youthIds = checkIns.Select(checkIn => checkIn.YouthUserId).Distinct().ToList();
        var youthNames = await _db.Users
            .Where(user => youthIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.DisplayName, cancellationToken);

        return checkIns.Select(checkIn => new CheckInSummary(
            checkIn.Id,
            checkIn.YouthUserId,
            youthNames.GetValueOrDefault(checkIn.YouthUserId, "Youth"),
            checkIn.Date,
            checkIn.Type,
            checkIn.Engagement,
            checkIn.GoalsDiscussed,
            checkIn.PrayerRequests,
            checkIn.ActionItems,
            checkIn.Visibility.ToString(),
            checkIn.NextFollowUp,
            checkIn.GuardianInformed,
            checkIn.CreatedAt)).ToList();
    }
}
