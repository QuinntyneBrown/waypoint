namespace WordUp.Domain.LearningExperience;

/// <summary>Private resumable code and self-check state for one youth and coding lesson.</summary>
public class CodingLessonWorkspace
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid YouthUserId { get; set; }
    public Guid LessonId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string CheckedStepIdsJson { get; set; } = "[]";
    public DateTime? CompletedAt { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
