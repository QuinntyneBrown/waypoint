namespace WordUp.Domain.Engagement;

/// <summary>A durable future notification generated from an RSVP or another dated event.</summary>
public class ScheduledNotification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string EventKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? LinkPath { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public Guid SourceId { get; set; }
    public DateTime DeliverAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int AttemptCount { get; set; }
    public DateTime? LastAttemptAt { get; set; }
    public DateTime? NextAttemptAt { get; set; }
    public Guid? LeaseId { get; set; }
    public DateTime? LeaseUntil { get; set; }
    public string? LastErrorCode { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
