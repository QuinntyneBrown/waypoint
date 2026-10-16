namespace Waypoint.Domain.PlatformServices;

/// <summary>A user's display &amp; accessibility preferences (L2-PLATFORM-004). One row per user.
/// Language selection is out of scope. Theme is system, light, or dark (L2-PLATFORM-013).</summary>
public class DisplayPreference
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public int TextScalePercent { get; set; } = 100; // 80..200
    public bool HighContrast { get; set; }
    public bool ReduceMotion { get; set; }
    public bool UnderlineLinks { get; set; }
    public string? ReadingFont { get; set; }
    public string Theme { get; set; } = "system";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
