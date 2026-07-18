namespace WordUp.Domain.CurriculumAuthoring;

/// <summary>A quiz attached to a module or lesson (L2-CURRIC-003). Recorded score = best passing
/// attempt within the attempt limit (L2-LEARN-004).</summary>
public class Quiz
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? ModuleId { get; set; }
    public Guid? LessonId { get; set; }
    public int PassMark { get; set; } = 70;
    public int AttemptsAllowed { get; set; } = 2;
    public string FeedbackMode { get; set; } = "show-correct"; // show-correct | score-only | after-all-attempts
    public bool Shuffle { get; set; }
    public bool RequiredToCompleteModule { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
