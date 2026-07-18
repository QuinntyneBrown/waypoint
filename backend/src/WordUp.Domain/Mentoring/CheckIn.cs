namespace WordUp.Domain.Mentoring;

/// <summary>A mentor's check-in with a youth (L2-MENTOR-006). Private by default; prayer requests stay
/// mentor-only unless sharing is explicitly enabled.</summary>
public class CheckIn
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid YouthUserId { get; set; }
    public Guid MentorUserId { get; set; }
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty; // 1:1 | call | session
    public string? Engagement { get; set; }
    public string? GoalsDiscussed { get; set; }
    public string? PrayerRequests { get; set; }
    public string? ActionItems { get; set; }
    public DateTime? NextFollowUp { get; set; }
    public Visibility Visibility { get; set; } = Visibility.Private;
    public bool GuardianInformed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
