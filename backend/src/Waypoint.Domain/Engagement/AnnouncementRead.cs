namespace Waypoint.Domain.Engagement;

/// <summary>Records that a recipient opened an announcement (L2-ENGAGE-006 AC3). One row per
/// (announcement, user); aggregate read rate is computed from these.</summary>
public class AnnouncementRead
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AnnouncementId { get; set; }
    public Guid UserId { get; set; }
    public DateTime ReadAt { get; set; } = DateTime.UtcNow;
}
