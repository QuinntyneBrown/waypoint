namespace WordUp.Domain.LearningExperience;

/// <summary>A youth's post-lesson reflection (L2-LEARN-003). Private by default; drafts supported.</summary>
public class ReflectionEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid YouthUserId { get; set; }
    public Guid LessonId { get; set; }
    public string? Text { get; set; }
    public string? Mood { get; set; }
    public string? SecondaryJson { get; set; }
    public Visibility Visibility { get; set; } = Visibility.Private;
    public bool IsDraft { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
