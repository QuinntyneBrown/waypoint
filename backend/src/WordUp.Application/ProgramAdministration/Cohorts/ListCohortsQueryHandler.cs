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
            .OrderBy(c => c.Name)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.TrackId,
                c.MentorId,
                MemberCount = _db.Users.Count(u => u.CohortId == c.Id && u.Role == Roles.Youth),
            })
            .ToListAsync(cancellationToken);

        return cohorts
            .Select(c => new CohortSummary(c.Id, c.Name, c.TrackId, c.MentorId, c.MemberCount))
            .ToList();
    }
}
