namespace WordUp.Domain.PublicSite;

/// <summary>
/// A public contact-form submission (L2-PUB-003 AC1). Free-text is stored HTML-encoded to neutralise
/// stored XSS (L2-NFR-001).
/// </summary>
public class ContactRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
