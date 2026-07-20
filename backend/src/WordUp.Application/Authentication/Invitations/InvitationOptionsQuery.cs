using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Authentication.Invitations;

public sealed record InvitationOptionsQuery : IRequest<InvitationOptions>;
public sealed record InvitationOptions(
    IReadOnlyList<InvitationCohortOption> Cohorts,
    IReadOnlyList<InvitationMentorOption> Mentors);
public sealed record InvitationCohortOption(Guid Id, string Name);
public sealed record InvitationMentorOption(Guid Id, string DisplayName);

public sealed class InvitationOptionsQueryHandler
    : IRequestHandler<InvitationOptionsQuery, InvitationOptions>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public InvitationOptionsQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<InvitationOptions> Handle(
        InvitationOptionsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException();
        IQueryable<Cohort> cohorts = _db.Cohorts.AsNoTracking();
        IQueryable<User> mentors = _db.Users.AsNoTracking()
            .Where(item => item.Role == Roles.Mentor && item.Status == UserStatus.Active);
        if (_currentUser.Role == Roles.Mentor)
        {
            var supportingIds = _db.CohortMentors
                .Where(item => item.MentorId == userId)
                .Select(item => item.CohortId);
            cohorts = cohorts.Where(item => item.MentorId == userId || supportingIds.Contains(item.Id));
            mentors = mentors.Where(item => item.Id == userId);
        }
        else if (_currentUser.Role != Roles.ProgramAdministrator)
        {
            throw new ForbiddenException();
        }

        return new InvitationOptions(
            await cohorts.OrderBy(item => item.Name)
                .Select(item => new InvitationCohortOption(item.Id, item.Name))
                .ToListAsync(cancellationToken),
            await mentors.OrderBy(item => item.DisplayName)
                .Select(item => new InvitationMentorOption(item.Id, item.DisplayName))
                .ToListAsync(cancellationToken));
    }
}
