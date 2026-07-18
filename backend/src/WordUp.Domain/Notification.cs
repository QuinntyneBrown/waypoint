namespace WordUp.Domain;

/// <summary>A per-user in-app notification (L2-ENGAGE-008). Carries a title, body, and a deep link to
/// its target screen; generation respects the user's notification preferences (L2-PLATFORM-003).</summary>
public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string EventKey { get; set; } = string.Empty; // aligns with NotificationPreference.EventKey
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? LinkPath { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
