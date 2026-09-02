namespace Waypoint.Domain.PlatformServices;

/// <summary>A user's per-event notification preference (L2-PLATFORM-003). In-app is the primary
/// channel; email is optional. One row per (user, event). Absence of a row means "use the default"
/// (in-app on and email off).</summary>
public class NotificationPreference
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string EventKey { get; set; } = string.Empty; // e.g. lesson-reminders | work-graded | mentor-messages
    public bool InApp { get; set; } = true;
    public bool Email { get; set; }
}
