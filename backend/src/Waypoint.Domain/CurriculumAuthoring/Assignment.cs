namespace Waypoint.Domain.CurriculumAuthoring;

/// <summary>A curriculum assignment definition (L2-CURRIC-004).</summary>
public class Assignment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? ModuleId { get; set; }
    public Guid? LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string TaskDescription { get; set; } = string.Empty;
    public string SubmissionTypes { get; set; } = string.Empty; // csv of: text,photo,link,file
    public DateTime? DueDate { get; set; }
    public int GracePeriodDays { get; set; }
    public bool AllowLate { get; set; }
    public bool RequireMentorApproval { get; set; }
    public int TotalPoints { get; set; }
    public PublishStatus Status { get; set; } = PublishStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
