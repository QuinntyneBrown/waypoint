namespace WordUp.Domain.PlatformServices;

/// <summary>A user-submitted help/support request (L2-PLATFORM-005). Persisted and emailed to the
/// program support inbox (the director).</summary>
public class SupportRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? UserId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ReplyToEmail { get; set; } = string.Empty;
    public string? ScreenshotUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
