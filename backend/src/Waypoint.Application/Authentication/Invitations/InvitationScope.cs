using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.Authentication.Invitations;

internal static class InvitationScope
{
    public static IQueryable<Invitation> Apply(
        IQueryable<Invitation> query,
        ICurrentUser currentUser)
    {
        if (currentUser.Role == Roles.ProgramAdministrator) return query;
        var userId = currentUser.UserId ?? throw new ForbiddenException();
        if (currentUser.Role != Roles.Mentor) throw new ForbiddenException();
        return query.Where(item => item.CreatedByUserId == userId && item.Role == Roles.Youth);
    }

    public static async Task<(Guid? CohortId, Guid? MentorId)> ValidateAssignmentAsync(
        IAppDbContext db,
        ICurrentUser currentUser,
        string role,
        Guid? cohortId,
        Guid? mentorId,
        CancellationToken cancellationToken)
    {
        if (currentUser.Role == Roles.Mentor)
        {
            var currentMentorId = currentUser.UserId ?? throw new ForbiddenException();
            if (role != Roles.Youth) throw new ForbiddenException("Mentors may only invite youth.");
            if (mentorId is not null && mentorId != currentMentorId)
            {
                throw new ForbiddenException("Mentors may only assign invitations to themselves.");
            }
            if (cohortId is Guid requestedCohortId)
            {
                var assigned = await db.Cohorts.AsNoTracking().AnyAsync(
                    item => item.Id == requestedCohortId && item.MentorId == currentMentorId,
                    cancellationToken)
                    || await db.CohortMentors.AsNoTracking().AnyAsync(
                        item => item.CohortId == requestedCohortId && item.MentorId == currentMentorId,
                        cancellationToken);
                if (!assigned) throw new ForbiddenException("That cohort is outside your assignment scope.");
            }
            return (cohortId, currentMentorId);
        }

        if (currentUser.Role != Roles.ProgramAdministrator) throw new ForbiddenException();
        if (cohortId is Guid selectedCohortId
            && !await db.Cohorts.AsNoTracking().AnyAsync(item => item.Id == selectedCohortId, cancellationToken))
        {
            throw new NotFoundException("Cohort not found.");
        }
        if (mentorId is Guid selectedMentorId
            && !await db.Users.AsNoTracking().AnyAsync(
                item => item.Id == selectedMentorId && item.Role == Roles.Mentor,
                cancellationToken))
        {
            throw new NotFoundException("Mentor not found.");
        }
        return (cohortId, mentorId);
    }

    public static async Task<Invitation> LoadManagedAsync(
        IAppDbContext db,
        ICurrentUser currentUser,
        Guid invitationId,
        CancellationToken cancellationToken)
    {
        var invitation = await Apply(db.Invitations, currentUser)
            .FirstOrDefaultAsync(item => item.Id == invitationId, cancellationToken);
        return invitation ?? throw new NotFoundException("Invitation not found.");
    }
}
