using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.ProgramAdministration.Cohorts;

public class CreateCohortCommandHandler : IRequestHandler<CreateCohortCommand, CohortResult>
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUser _currentUser;

    public CreateCohortCommandHandler(IAppDbContext db, IAuditLogger audit, ICurrentUser currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<CohortResult> Handle(CreateCohortCommand request, CancellationToken cancellationToken)
    {
        var trackExists = await _db.Tracks.AnyAsync(
            track => track.Id == request.TrackId
                && track.Enabled
                && track.Status == PublishStatus.Published,
            cancellationToken);
        if (!trackExists)
        {
            throw new ConflictException("Choose an enabled, published curriculum track.");
        }

        var leadMentorExists = await _db.Users.AnyAsync(
            user => user.Id == request.LeadMentorId
                && user.Role == Roles.Mentor
                && user.Status == UserStatus.Active,
            cancellationToken);
        if (!leadMentorExists)
        {
            throw new ConflictException("Choose an active mentor as cohort lead.");
        }

        var cohort = new Cohort
        {
            Name = request.Name.Trim(),
            TrackId = request.TrackId,
            MentorId = request.LeadMentorId,
            TermStart = request.StartDate ?? default,
            TermEnd = request.EndDate ?? default,
            Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim(),
        };

        _db.Cohorts.Add(cohort);
        _db.CohortMentors.Add(new CohortMentor
        {
            CohortId = cohort.Id,
            MentorId = request.LeadMentorId,
            IsLead = true,
        });

        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync("CohortCreated", _currentUser.UserId, "Cohort", cohort.Id);

        return new CohortResult(cohort.Id);
    }
}
