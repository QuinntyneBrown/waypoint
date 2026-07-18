namespace WordUp.Domain.Authentication;

/// <summary>
/// A rotating, hashed-at-rest refresh token (L2-AUTH-012). Tokens are chained within a
/// <see cref="FamilyId"/>; reuse of a rotated token revokes the whole family (reuse detection).
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid FamilyId { get; set; } = Guid.NewGuid();
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByTokenHash { get; set; }

    public bool IsActive(DateTime utcNow) => RevokedAt is null && ExpiresAt > utcNow;
}
