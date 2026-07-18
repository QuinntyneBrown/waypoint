using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.ProgramAdministration.Cohorts;

public class AddYouthToCohortCommandHandler : IRequestHandler<AddYouthToCohortCommand>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public AddYouthToCohortCommandHandler(
        IAppDbContext db,
        IAuditLogger audit,
        ICurrentUser currentUser,
        IClock clock)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task Handle(AddYouthToCohortCommand request, CancellationToken cancellationToken)
    {
        var cohort = await _db.Cohorts.FirstOrDefaultAsync(c => c.Id == request.CohortId, cancellationToken)
            ?? throw new NotFoundException("Cohort not found.");
        if (cohort.TrackId is { } cohortTrackId
            && !await _db.Tracks.AnyAsync(
                track => track.Id == cohortTrackId
                    && track.Enabled
                    && track.Status == PublishStatus.Published,
                cancellationToken))
        {
            throw new ConflictException("This cohort's track is paused for new enrolments.");
        }

        var youth = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.YouthId, cancellationToken)
            ?? throw new NotFoundException("User not found.");
        if (youth.Role != Roles.Youth || youth.Status != UserStatus.Active)
        {
            throw new ConflictException("Choose an active youth account.");
        }
        if (youth.CohortId is { } existingCohortId && existingCohortId != cohort.Id)
        {
            throw new ConflictException("This youth already belongs to another cohort.");
        }

        youth.CohortId = cohort.Id;
        if (cohort.TrackId is { } trackId
            && !await _db.YouthTrackEnrollments.AnyAsync(
                item => item.YouthUserId == youth.Id && item.TrackId == trackId,
                cancellationToken))
        {
            _db.YouthTrackEnrollments.Add(new YouthTrackEnrollment
            {
                YouthUserId = youth.Id,
                TrackId = trackId,
                EnrolledAt = _clock.UtcNow,
            });
        }
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("CohortYouthAdded", _currentUser.UserId, "Cohort", cohort.Id);
    }
}
