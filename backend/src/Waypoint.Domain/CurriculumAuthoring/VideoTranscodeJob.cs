namespace Waypoint.Domain.CurriculumAuthoring;

/// <summary>
/// Leased queue row for transcoding one source video. A worker claims it with an expiring lease and
/// extends the lease while it works, so a crashed host is detected when the lease lapses.
/// </summary>
public sealed class VideoTranscodeJob
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MediaId { get; set; }
    public int AttemptCount { get; set; }
    public Guid? LeaseId { get; set; }
    public DateTime? LeaseUntil { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? LastProgressAt { get; set; }
    public string? LastError { get; set; }
}
