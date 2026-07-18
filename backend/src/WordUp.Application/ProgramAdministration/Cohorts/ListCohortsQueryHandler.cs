using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.ProgramAdministration.Cohorts;

public class ListCohortsQueryHandler : IRequestHandler<ListCohortsQuery, IReadOnlyList<CohortSummary>>
{
    private readonly IAppDbContext _db;

    public ListCohortsQueryHandler(IAppDbContext db) => _db = db;

    public async Task<IReadOnlyList<CohortSummary>> Handle(ListCohortsQuery request, CancellationToken cancellationToken)
    {
        var cohorts = await _db.Cohorts
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.TrackId,
                TrackName = _db.Tracks
                    .Where(track => track.Id == c.TrackId)
                    .Select(track => track.Name)
                    .FirstOrDefault(),
                c.MentorId,
                MentorName = _db.Users
                    .Where(mentor => mentor.Id == c.MentorId)
                    .Select(mentor => mentor.DisplayName)
                    .FirstOrDefault(),
                c.TermStart,
                c.TermEnd,
                c.Location,
                MemberCount = _db.Users.Count(u =>
                    u.CohortId == c.Id
                    && u.Role == Roles.Youth
                    && u.Status != UserStatus.Removed),
            })
            .ToListAsync(cancellationToken);

        return cohorts
            .Select(c => new CohortSummary(
                c.Id,
                c.Name,
                c.TrackId,
                c.TrackName,
                c.MentorId,
                c.MentorName,
                c.TermStart,
                c.TermEnd,
                c.Location,
                c.MemberCount))
            .ToList();
    }
}
