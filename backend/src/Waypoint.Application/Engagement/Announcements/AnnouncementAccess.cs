using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.Engagement.Announcements;

/// <summary>One targeting boundary for delivery, recipient visibility, and read statistics.</summary>
internal static class AnnouncementAccess
{
    public static async Task ActivateDueAsync(
        IAppDbContext db,
        IClock clock,
        CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var due = await db.Announcements
            .Where(item => item.SentAt == null
                && item.ScheduledAt != null
                && item.ScheduledAt <= now)
            .ToListAsync(cancellationToken);

        if (due.Count == 0) return;

        foreach (var announcement in due)
        {
            announcement.SentAt = announcement.ScheduledAt;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public static async Task<IQueryable<Announcement>> VisibleToAsync(
        IAppDbContext db,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");
        var query = db.Announcements.Where(item => item.SentAt != null);
        if (user.Role == Roles.ProgramAdministrator) return query;

        var cohortIds = await CohortIdsForAsync(db, user, cancellationToken);
        query = cohortIds.Count == 0
            ? query.Where(item => item.AudienceScope == "org")
            : query.Where(item => item.AudienceScope == "org"
                || (item.CohortId != null && cohortIds.Contains(item.CohortId.Value)));

        var allowedTargets = MyAnnouncementsQueryHandler.TargetsForRole(user.Role);
        return query.Where(item => allowedTargets.Contains(item.RoleTargeting));
    }

    public static async Task<IReadOnlyList<Guid>> RecipientIdsAsync(
        IAppDbContext db,
        Announcement announcement,
        CancellationToken cancellationToken)
    {
        var users = await db.Users.AsNoTracking()
            .Where(item => item.Status == UserStatus.Active)
            .ToListAsync(cancellationToken);
        IEnumerable<User> candidates = users.Where(item => MatchesRole(item.Role, announcement.RoleTargeting));

        if (announcement.AudienceScope == "org")
        {
            return candidates.Select(item => item.Id).Distinct().ToList();
        }

        if (announcement.CohortId is not Guid cohortId) return [];

        var audienceIds = users
            .Where(item => item.CohortId == cohortId)
            .Select(item => item.Id)
            .ToHashSet();

        foreach (var guardianId in users
            .Where(item => item.Role == Roles.Youth
                && item.CohortId == cohortId
                && item.GuardianUserId != null)
            .Select(item => item.GuardianUserId!.Value))
        {
            audienceIds.Add(guardianId);
        }

        var mentorIds = await db.CohortMentors.AsNoTracking()
            .Where(item => item.CohortId == cohortId)
            .Select(item => item.MentorId)
            .ToListAsync(cancellationToken);
        foreach (var mentorId in mentorIds) audienceIds.Add(mentorId);

        var leadMentorId = await db.Cohorts.AsNoTracking()
            .Where(item => item.Id == cohortId)
            .Select(item => item.MentorId)
            .FirstOrDefaultAsync(cancellationToken);
        if (leadMentorId is Guid leadId) audienceIds.Add(leadId);

        var volunteerIds = await db.Sessions.AsNoTracking()
            .Where(item => item.CohortId == cohortId && item.VolunteerId != null)
            .Select(item => item.VolunteerId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);
        foreach (var volunteerId in volunteerIds) audienceIds.Add(volunteerId);

        // Program Administrators administer every cohort and receive cohort-wide "Everyone" items.
        foreach (var adminId in users
            .Where(item => item.Role == Roles.ProgramAdministrator)
            .Select(item => item.Id))
        {
            audienceIds.Add(adminId);
        }

        return candidates
            .Where(item => audienceIds.Contains(item.Id))
            .Select(item => item.Id)
            .Distinct()
            .ToList();
    }

    public static async Task<AnnouncementStats> StatsAsync(
        IAppDbContext db,
        Announcement announcement,
        CancellationToken cancellationToken)
    {
        var recipientIds = await RecipientIdsAsync(db, announcement, cancellationToken);
        var reads = recipientIds.Count == 0
            ? 0
            : await db.AnnouncementReads.AsNoTracking()
                .CountAsync(item => item.AnnouncementId == announcement.Id
                    && recipientIds.Contains(item.UserId), cancellationToken);
        var readRate = recipientIds.Count == 0 ? 0d : (double)reads / recipientIds.Count;
        return new AnnouncementStats(recipientIds.Count, reads, readRate);
    }

    private static bool MatchesRole(string role, string targeting) => targeting switch
    {
        "Youth" => role == Roles.Youth,
        "Guardians" => role == Roles.Guardian,
        "YouthGuardians" => role is Roles.Youth or Roles.Guardian,
        "Mentors" => role == Roles.Mentor,
        "Everyone" => true,
        _ => false,
    };

    private static async Task<List<Guid>> CohortIdsForAsync(
        IAppDbContext db,
        User user,
        CancellationToken cancellationToken)
    {
        if (user.Role == Roles.Guardian)
        {
            return await db.Users.AsNoTracking()
                .Where(item => item.GuardianUserId == user.Id && item.CohortId != null)
                .Select(item => item.CohortId!.Value)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        if (user.Role == Roles.Mentor)
        {
            var cohortIds = await db.CohortMentors.AsNoTracking()
                .Where(item => item.MentorId == user.Id)
                .Select(item => item.CohortId)
                .ToListAsync(cancellationToken);
            var leadIds = await db.Cohorts.AsNoTracking()
                .Where(item => item.MentorId == user.Id)
                .Select(item => item.Id)
                .ToListAsync(cancellationToken);
            return cohortIds.Concat(leadIds).Distinct().ToList();
        }

        if (user.Role == Roles.Volunteer)
        {
            return await db.Sessions.AsNoTracking()
                .Where(item => item.VolunteerId == user.Id)
                .Select(item => item.CohortId)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        return user.CohortId is Guid cohortId ? [cohortId] : [];
    }
}
