namespace WordUp.Domain.LearningExperience;

/// <summary>A youth's assignment (or coding-project) submission and its review lifecycle
/// (L2-LEARN-006/007, L2-MENTOR-003).</summary>
public class Submission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid YouthUserId { get; set; }
    public Guid AssignmentId { get; set; }
    public string? BodyText { get; set; }
    public Guid? PhotoUploadId { get; set; }
    public string? PhotoUrl { get; set; }
    public Guid? FileUploadId { get; set; }
    public string? LinkUrl { get; set; }
    public bool IsCodingProject { get; set; }
    public string? YouthNote { get; set; }

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;
    public string? MentorComment { get; set; }
    public string? RubricScoresJson { get; set; }
    public int? TotalAwarded { get; set; }
    public bool GuardianNotified { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
