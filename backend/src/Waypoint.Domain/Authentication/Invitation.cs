namespace Waypoint.Domain.Authentication;

/// <summary>
/// A role-scoped, single-use, tokenized invitation (L2-AUTH-003 / L2-AUTH-004).
/// The token is stored hashed at rest; expiry is 14 days from issuance by default.
/// </summary>
public class Invitation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = Roles.Youth;
    public Guid? CohortId { get; set; }
    public Guid? MentorId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? CreatedByUserId { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? ReplacementRequestedAt { get; set; }
    public Guid ConcurrencyStamp { get; set; } = Guid.NewGuid();

    public bool IsAcceptable(DateTime utcNow) =>
        Status == InvitationStatus.Pending && ExpiresAt > utcNow;
}
