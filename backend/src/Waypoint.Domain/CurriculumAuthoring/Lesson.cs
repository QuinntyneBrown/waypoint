namespace Waypoint.Domain.CurriculumAuthoring;

/// <summary>A lesson within a module (L2-CURRIC-002). Leader-guide notes are mentor-only.</summary>
public class Lesson
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ModuleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ScriptureReference { get; set; }
    public int? EstimatedMinutes { get; set; }
    public string? LearningObjectives { get; set; }
    public string? TeachingBody { get; set; }
    public string? ReflectionPrompts { get; set; }
    public string? LeaderGuideNotes { get; set; }
    public bool IsCodingLesson { get; set; }
    public string? StarterCode { get; set; }
    public int Order { get; set; }
    public PublishStatus Status { get; set; } = PublishStatus.Draft;
    public DateTime? ArchivedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
