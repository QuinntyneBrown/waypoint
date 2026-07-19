using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.ReportingAndData.Reports;

public record ReportOptionsQuery() : IRequest<ReportOptions>;

public sealed class ReportOptionsQueryHandler : IRequestHandler<ReportOptionsQuery, ReportOptions>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ReportOptionsQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ReportOptions> Handle(ReportOptionsQuery request, CancellationToken cancellationToken)
    {
        var youth = await ReportData.YouthAsync(_db, _currentUser, new ReportFilter(), cancellationToken);
        var youthIds = youth.Select(item => item.Id).ToList();
        var cohortIds = youth.Where(item => item.CohortId != null)
            .Select(item => item.CohortId!.Value)
            .Distinct()
            .ToList();
        var cohortsQuery = _db.Cohorts.AsNoTracking();
        if (_currentUser.Role == Roles.Mentor)
            cohortsQuery = cohortsQuery.Where(item => cohortIds.Contains(item.Id));
        var cohorts = await cohortsQuery.OrderBy(item => item.Name)
            .Select(item => new ReportOption(item.Id, item.Name))
            .ToListAsync(cancellationToken);

        var tracksQuery = _db.Tracks.AsNoTracking().Where(item => item.Status == PublishStatus.Published);
        if (_currentUser.Role == Roles.Mentor)
        {
            var primaryTrackIds = await _db.Cohorts.AsNoTracking()
                .Where(item => cohortIds.Contains(item.Id) && item.TrackId != null)
                .Select(item => item.TrackId!.Value)
                .ToListAsync(cancellationToken);
            var enrolledTrackIds = await _db.YouthTrackEnrollments.AsNoTracking()
                .Where(item => youthIds.Contains(item.YouthUserId))
                .Select(item => item.TrackId)
                .ToListAsync(cancellationToken);
            var trackIds = primaryTrackIds.Concat(enrolledTrackIds).Distinct().ToList();
            tracksQuery = tracksQuery.Where(item => trackIds.Contains(item.Id));
        }
        var tracks = await tracksQuery.OrderBy(item => item.Name)
            .Select(item => new ReportOption(item.Id, item.Name))
            .ToListAsync(cancellationToken);

        var mentorsQuery = _db.Users.AsNoTracking()
            .Where(item => item.Role == Roles.Mentor && item.Status == UserStatus.Active);
        if (_currentUser.Role == Roles.Mentor)
            mentorsQuery = mentorsQuery.Where(item => item.Id == _currentUser.UserId);
        var mentors = await mentorsQuery.OrderBy(item => item.DisplayName)
            .Select(item => new ReportOption(item.Id, item.DisplayName))
            .ToListAsync(cancellationToken);

        return new ReportOptions(
            cohorts,
            tracks,
            mentors,
            youth.Select(item => new ReportOption(item.Id, item.DisplayName)).ToList());
    }
}
