using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Application.ProgramAdministration.Cohorts;

public sealed class CohortOptionsQueryHandler : IRequestHandler<CohortOptionsQuery, CohortOptions>
{
    private readonly IAppDbContext _db;

    public CohortOptionsQueryHandler(IAppDbContext db) => _db = db;

    public async Task<CohortOptions> Handle(
        CohortOptionsQuery request,
        CancellationToken cancellationToken)
    {
        var tracks = await _db.Tracks
            .AsNoTracking()
            .Where(item => item.Enabled && item.Status == PublishStatus.Published)
            .OrderBy(item => item.Name)
            .Select(item => new CohortOption(item.Id, item.Name))
            .ToListAsync(cancellationToken);
        var mentors = await _db.Users
            .AsNoTracking()
            .Where(item => item.Role == Roles.Mentor && item.Status == UserStatus.Active)
            .OrderBy(item => item.DisplayName)
            .Select(item => new CohortOption(item.Id, item.DisplayName))
            .ToListAsync(cancellationToken);
        return new CohortOptions(tracks, mentors);
    }
}
