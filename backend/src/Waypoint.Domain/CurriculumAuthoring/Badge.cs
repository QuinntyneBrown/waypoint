namespace Waypoint.Domain.CurriculumAuthoring;

/// <summary>A badge definition (L2-CURRIC-005). Auto badges specify a triggering rule; manual badges
/// are awarded by a mentor/administrator.</summary>
public class Badge
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string? Description { get; set; }
    public BadgeCriteriaKind CriteriaKind { get; set; } = BadgeCriteriaKind.Manual;
    public string? AutoRule { get; set; } // e.g. module-completed | lesson-completed | quiz-passed | attendance-streak | assignment-approved
    public Guid? TargetRefId { get; set; } // the module/lesson/quiz the auto rule targets, when applicable
    public Guid? CompetencyId { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
