namespace WordUp.Domain;

/// <summary>A badge awarded to a youth (once). L2-CURRIC-005; auto-award evaluation happens in LEARN.</summary>
public class BadgeAward
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BadgeId { get; set; }
    public Guid YouthUserId { get; set; }
    public Guid? AwardedByUserId { get; set; }
    public DateTime AwardedAt { get; set; } = DateTime.UtcNow;
}
