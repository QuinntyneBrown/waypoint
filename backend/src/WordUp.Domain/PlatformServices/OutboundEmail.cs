namespace WordUp.Domain.PlatformServices;

/// <summary>
/// Encrypted durable transactional-email envelope. Delivery uses a database lease so multiple API
/// instances can safely drain the same outbox; the stable Id is also used as the SMTP Message-Id.
/// </summary>
public sealed class OutboundEmail
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    /// <summary>The account whose current status and address authorize delivery.</summary>
    public Guid? RecipientUserId { get; set; }
    /// <summary>The person whose private content the envelope concerns; may differ from recipient.</summary>
    public Guid? SubjectUserId { get; set; }
    /// <summary>Narrow exception for the one final removal confirmation sent to the former address.</summary>
    public bool IsPrivacyRemovalConfirmation { get; set; }
    public bool PurgeAfterDelivery { get; set; }
    public string? IdempotencyKey { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime AvailableAt { get; set; } = DateTime.UtcNow;
    public int AttemptCount { get; set; }
    public DateTime? LastAttemptAt { get; set; }
    public DateTime? SentAt { get; set; }
    public Guid? LeaseId { get; set; }
    public DateTime? LeaseUntil { get; set; }
    public string? LastErrorCode { get; set; }
}
