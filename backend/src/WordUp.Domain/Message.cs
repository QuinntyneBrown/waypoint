namespace WordUp.Domain;

/// <summary>A 1:1 direct message (L2-ENGAGE-005/ENGAGE-007). A conversation is keyed by
/// <see cref="ThreadKey"/> = the two participant ids ordered and joined, so both directions land in
/// the same thread. A mentor broadcast fans out into one Message per youth. Body is stored
/// HTML-encoded to neutralise stored XSS (L2-NFR-001).</summary>
public class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ThreadKey { get; set; } = string.Empty;
    public Guid SenderId { get; set; }
    public Guid RecipientId { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }

    /// <summary>Deterministic thread key for a participant pair (order-independent).</summary>
    public static string KeyFor(Guid a, Guid b)
    {
        var (first, second) = a.CompareTo(b) <= 0 ? (a, b) : (b, a);
        return $"{first:N}:{second:N}";
    }
}
