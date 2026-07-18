namespace WordUp.Domain.Mentoring;

/// <summary>A mentor's assessment of a youth on a competency (L2-MENTOR-005).</summary>
public class CompetencyAssessment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid YouthUserId { get; set; }
    public Guid CompetencyId { get; set; }
    public CompetencyLevel Level { get; set; } = CompetencyLevel.NotStarted;
    public string? Notes { get; set; }
    public Guid AssessedByUserId { get; set; }
    public DateTime AssessedAt { get; set; } = DateTime.UtcNow;
}
