namespace Waypoint.Domain.CurriculumAuthoring;

/// <summary>An ordered stage in the progression framework (L2-CURRIC-006).</summary>
public class ProgressionStage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
