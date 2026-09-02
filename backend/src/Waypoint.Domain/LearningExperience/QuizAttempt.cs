namespace Waypoint.Domain.LearningExperience;

/// <summary>A youth's quiz attempt (L2-LEARN-004/005, L2-MENTOR-004). Immutable once submitted; the
/// mentor-approved score is what counts and is what the youth sees.</summary>
public class QuizAttempt
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid YouthUserId { get; set; }
    public Guid QuizId { get; set; }
    public int AttemptNumber { get; set; }
    public string? AnswersJson { get; set; }
    public int CurrentQuestionIndex { get; set; }
    public int AutoScore { get; set; }
    public bool Passed { get; set; }
    public bool IsSubmitted { get; set; }
    public DateTime? SubmittedAt { get; set; }

    // Mentor approval / override (L2-MENTOR-004). Youth sees ApprovedScore only once approved.
    public int? ApprovedScore { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
