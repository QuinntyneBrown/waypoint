namespace WordUp.Domain.ConsentAndSafeguarding;

/// <summary>
/// Append-only audit record (L2-CONSENT-003). Written for mutating admin/safeguarding actions.
/// Private check-in note content is never stored here; <see cref="IsPrivate"/> marks entries whose
/// detail must be redacted in the log view.
/// </summary>
public class AuditEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? ActorUserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? TargetType { get; set; }
    public Guid? TargetId { get; set; }
    public DateTime At { get; set; } = DateTime.UtcNow;
    public string? Source { get; set; }
    public bool IsPrivate { get; set; }
    public string? Details { get; set; }
}
