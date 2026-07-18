namespace WordUp.Domain.LearningExperience;

/// <summary>Immutable youth-visible lifecycle history for an assignment submission.</summary>
public sealed class SubmissionTimelineEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SubmissionId { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
