namespace WordUp.Domain.Engagement;

/// <summary>A youth's RSVP to a session/event (L2-ENGAGE-005). One RSVP per (session, youth); an
/// optional note is delivered to the mentor. Capacity/waitlisting is out of scope.</summary>
public class SessionRsvp
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SessionId { get; set; }
    public Guid YouthUserId { get; set; }
    public RsvpResponse Response { get; set; }
    public string? Note { get; set; }
    public bool ReminderSet { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
