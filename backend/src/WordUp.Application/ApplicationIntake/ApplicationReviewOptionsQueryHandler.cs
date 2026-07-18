using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.ApplicationIntake;

public sealed class ApplicationReviewOptionsQueryHandler
    : IRequestHandler<ApplicationReviewOptionsQuery, ApplicationReviewOptions>
{
    private readonly IAppDbContext _db;

    public ApplicationReviewOptionsQueryHandler(IAppDbContext db) => _db = db;

    public async Task<ApplicationReviewOptions> Handle(
        ApplicationReviewOptionsQuery request,
        CancellationToken cancellationToken)
    {
        var cohorts = await _db.Cohorts.AsNoTracking()
            .Where(item => item.TrackId == null || _db.Tracks.Any(track =>
                track.Id == item.TrackId
                && track.Enabled
                && track.Status == PublishStatus.Published))
            .OrderBy(item => item.Name)
            .Select(item => new { item.Id, item.Name, item.MentorId })
            .ToListAsync(cancellationToken);
        var cohortMentors = await _db.CohortMentors.AsNoTracking()
            .Select(item => new { item.CohortId, item.MentorId })
            .ToListAsync(cancellationToken);
        var mentors = await _db.Users.AsNoTracking()
            .Where(item => item.Role == Roles.Mentor && item.Status == UserStatus.Active)
            .OrderBy(item => item.DisplayName)
            .Select(item => new { item.Id, item.DisplayName })
            .ToListAsync(cancellationToken);
        var tracks = await _db.Tracks.AsNoTracking()
            .Where(item => item.Enabled && item.Status == PublishStatus.Published)
            .OrderBy(item => item.Name)
            .Select(item => new ApplicationOption(item.Id, item.Name))
            .ToListAsync(cancellationToken);

        return new ApplicationReviewOptions(
            cohorts.Select(item => new ApplicationOption(item.Id, item.Name)).ToList(),
            mentors.Select(mentor => new ApplicationMentorOption(
                mentor.Id,
                mentor.DisplayName,
                cohorts.Where(cohort => cohort.MentorId == mentor.Id)
                    .Select(cohort => cohort.Id)
                    .Concat(cohortMentors.Where(item => item.MentorId == mentor.Id)
                        .Select(item => item.CohortId))
                    .Distinct()
                    .ToList())).ToList(),
            tracks);
    }
}
