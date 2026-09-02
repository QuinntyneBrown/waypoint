namespace Waypoint.Domain.Engagement;

/// <summary>A scheduled session/event for a cohort (L2-ENGAGE-001). Weekly-recurring sessions are
/// materialised as individual rows generated through the cohort's term-end date and share a
/// <see cref="RecurrenceGroupId"/>.</summary>
public class Session
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CohortId { get; set; }
    public Guid? MentorId { get; set; }
    public Guid? VolunteerId { get; set; } // assigned volunteer (L2-VOLUNTEER-001/002)
    public string Title { get; set; } = string.Empty;
    public DateTime StartsAt { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Location { get; set; }

    // Recurrence (L2-ENGAGE-001 AC2). RepeatsWeekly rows generated from the start date through the
    // cohort term-end (inclusive); each generated row carries the shared group id.
    public bool RepeatsWeekly { get; set; }
    public Guid? RecurrenceGroupId { get; set; }

    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
