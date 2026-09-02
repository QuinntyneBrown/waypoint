namespace Waypoint.Domain.LearningExperience;

/// <summary>
/// Additional track enrollment for a youth. The cohort track remains the primary enrollment;
/// this join enables explicit multi-track participation and switching (L2-LEARN-001 AC4).
/// </summary>
public sealed class YouthTrackEnrollment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid YouthUserId { get; set; }
    public Guid TrackId { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
}
