namespace Waypoint.Domain.CurriculumAuthoring;

/// <summary>A data-driven curriculum track (e.g. Bible, Leadership, Coding) — L2-CURRIC-001.</summary>
public class Track
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PublishStatus Status { get; set; } = PublishStatus.Draft;
    public bool Enabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
