namespace Waypoint.Domain.Engagement;

/// <summary>A staff announcement targeted by audience + role (L2-ENGAGE-006). Sent immediately or
/// scheduled; per-recipient reads are tracked in <see cref="AnnouncementRead"/>.</summary>
public class Announcement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    // Audience scope: "org" (whole program) or "cohort" (CohortId set).
    public string AudienceScope { get; set; } = "org";
    public Guid? CohortId { get; set; }

    // Role targeting: Youth | YouthGuardians | Guardians | Mentors | Everyone.
    public string RoleTargeting { get; set; } = "Everyone";

    public DateTime? ScheduledAt { get; set; }
    public DateTime? SentAt { get; set; }

    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
