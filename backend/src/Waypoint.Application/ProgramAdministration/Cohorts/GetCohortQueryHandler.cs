using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.ProgramAdministration.Cohorts;

public sealed class GetCohortQueryHandler : IRequestHandler<GetCohortQuery, CohortDetail>
{
    private readonly IAppDbContext _db;

    public GetCohortQueryHandler(IAppDbContext db) => _db = db;

    public async Task<CohortDetail> Handle(GetCohortQuery request, CancellationToken cancellationToken)
    {
        var cohort = await _db.Cohorts
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Cohort not found.");
        var trackName = cohort.TrackId is { } trackId
            ? await _db.Tracks
                .AsNoTracking()
                .Where(item => item.Id == trackId)
                .Select(item => item.Name)
                .FirstOrDefaultAsync(cancellationToken)
            : null;
        var mentorName = cohort.MentorId is { } mentorId
            ? await _db.Users
                .AsNoTracking()
                .Where(item => item.Id == mentorId)
                .Select(item => item.DisplayName)
                .FirstOrDefaultAsync(cancellationToken)
            : null;
        var members = await _db.Users
            .AsNoTracking()
            .Where(item => item.CohortId == cohort.Id
                && item.Role == Roles.Youth
                && item.Status != UserStatus.Removed)
            .OrderBy(item => item.DisplayName)
            .Select(item => new CohortYouthSummary(
                item.Id,
                item.DisplayName,
                item.Email,
                item.Status.ToString()))
            .ToListAsync(cancellationToken);
        var availableYouth = await _db.Users
            .AsNoTracking()
            .Where(item => item.CohortId == null
                && item.Role == Roles.Youth
                && item.Status == UserStatus.Active)
            .OrderBy(item => item.DisplayName)
            .Select(item => new CohortYouthSummary(
                item.Id,
                item.DisplayName,
                item.Email,
                item.Status.ToString()))
            .ToListAsync(cancellationToken);

        return new CohortDetail(
            cohort.Id,
            cohort.Name,
            cohort.TrackId,
            trackName,
            cohort.MentorId,
            mentorName,
            cohort.TermStart,
            cohort.TermEnd,
            cohort.Location,
            members.Count,
            members,
            availableYouth);
    }
}
