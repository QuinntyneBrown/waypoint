namespace Waypoint.Domain.Mentoring;

/// <summary>Work assigned to a youth (a lesson, module, or project) by a mentor (L2-MENTOR-008).</summary>
public class AssignedWork
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ItemType { get; set; } = string.Empty; // Lesson | Module | Project
    public Guid ItemId { get; set; }
    public Guid YouthUserId { get; set; }
    public Guid? CohortId { get; set; }
    public DateTime? DueDate { get; set; }
    public string? ReminderCadence { get; set; }
    public string? Message { get; set; }
    public Guid AssignedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
