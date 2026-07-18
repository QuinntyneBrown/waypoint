namespace WordUp.Domain;

/// <summary>
/// A public volunteer sign-up request (L2-PUB-003 AC2). Submission is only accepted once the visitor
/// has acknowledged that volunteers working with youth must complete a screening and reference check
/// (<see cref="ScreeningConsentAck"/>). Distinct from the authenticated volunteer-portal role.
/// </summary>
public class VolunteerSignup
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? AreasOfInterest { get; set; }
    public bool ScreeningConsentAck { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
