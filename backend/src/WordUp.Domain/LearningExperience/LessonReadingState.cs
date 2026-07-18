namespace WordUp.Domain.LearningExperience;

/// <summary>Per-youth reading preferences/position for a lesson (L2-LEARN-002 AC2).</summary>
public class LessonReadingState
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid YouthUserId { get; set; }
    public Guid LessonId { get; set; }
    public string? TextSize { get; set; }
    public bool Bookmarked { get; set; }
    public int Position { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
