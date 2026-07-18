using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.Mentoring.CheckIns;

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

        return await query
            .OrderByDescending(c => c.Date)
            .Select(c => new CheckInSummary(
                c.Id,
                c.YouthUserId,
                c.Date,
                c.Type,
                c.Visibility.ToString(),
                c.NextFollowUp))
            .ToListAsync(cancellationToken);
    }
}
