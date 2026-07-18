namespace WordUp.Domain;

/// <summary>A youth's progress on a lesson (L2-LEARN-002). Single source of truth for completion.</summary>
public class LessonProgress
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid YouthUserId { get; set; }
    public Guid LessonId { get; set; }
    public ProgressStatus Status { get; set; } = ProgressStatus.NotStarted;
    public DateTime? CompletedAt { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
