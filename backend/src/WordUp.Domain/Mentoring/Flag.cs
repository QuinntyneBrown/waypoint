namespace WordUp.Domain.Mentoring;

/// <summary>An at-risk flag on a youth (L2-MENTOR-007). Visible to the director.</summary>
public class Flag
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid YouthUserId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid FlaggedByUserId { get; set; }
    public DateTime FlaggedAt { get; set; } = DateTime.UtcNow;
    public bool IsAutomatic { get; set; }
    public string? AutomaticReason { get; set; }
    public string? AutomaticSignalKeys { get; set; }
    public DateTime? AutomaticSignalsClearedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionReason { get; set; }

    public bool IsActive => ResolvedAt is null;
}
