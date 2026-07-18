using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Engagement.Sessions;

public sealed class SessionOptionsQueryHandler
    : IRequestHandler<SessionOptionsQuery, SessionSchedulingOptions>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public SessionOptionsQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<SessionSchedulingOptions> Handle(
        SessionOptionsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException();
        var cohorts = _db.Cohorts.AsNoTracking();
        if (_currentUser.Role == Roles.Mentor)
        {
            cohorts = cohorts.Where(cohort =>
                cohort.MentorId == userId
                || _db.CohortMentors.Any(assignment => assignment.CohortId == cohort.Id
                    && assignment.MentorId == userId)
                || _db.Users.Any(user => user.CohortId == cohort.Id
                    && user.MentorId == userId
                    && user.Role == Roles.Youth
                    && user.Status == UserStatus.Active));
        }

        var cohortOptions = await cohorts
            .OrderBy(cohort => cohort.Name)
            .Select(cohort => new SessionCohortOption(
                cohort.Id,
                cohort.Name,
                cohort.TermStart,
                cohort.TermEnd))
            .ToListAsync(cancellationToken);

        var mentors = _db.Users.AsNoTracking().Where(user =>
            user.Role == Roles.Mentor && user.Status == UserStatus.Active);
        if (_currentUser.Role == Roles.Mentor)
        {
            mentors = mentors.Where(user => user.Id == userId);
        }
        var mentorOptions = await mentors
            .OrderBy(user => user.DisplayName)
            .Select(user => new SessionMentorOption(user.Id, user.DisplayName))
            .ToListAsync(cancellationToken);

        return new SessionSchedulingOptions(cohortOptions, mentorOptions);
    }
}
