namespace Waypoint.Domain.Authentication;

/// <summary>Single-use password-reset token, hashed at rest (L2-AUTH-005).</summary>
public class PasswordResetToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UsedAt { get; set; }

    public bool IsUsable(DateTime utcNow) => UsedAt is null && ExpiresAt > utcNow;
}
