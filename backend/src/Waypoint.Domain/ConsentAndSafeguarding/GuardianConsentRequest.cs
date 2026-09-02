namespace Waypoint.Domain.ConsentAndSafeguarding;

/// <summary>
/// One-time, hashed-at-rest capability delivered to a linked guardian so consent can be completed
/// without exposing a youth account or requiring a pre-existing guardian session (L2-CONSENT-002).
/// </summary>
public sealed class GuardianConsentRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid YouthUserId { get; set; }
    public Guid GuardianUserId { get; set; }
    public string Channel { get; set; } = "email";
    public string Destination { get; set; } = string.Empty;
    public string TokenHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }

    public bool IsUsable(DateTime utcNow) => UsedAt is null && ExpiresAt > utcNow;
}
