namespace Waypoint.Domain.GuardianPortal;

/// <summary>
/// A guardian's own contact/relationship details, editable in the guardian portal (L2-GUARDIAN-003).
/// One row per guardian <see cref="User"/>; the guardian's name and email live on the User itself.
/// </summary>
public class GuardianProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GuardianUserId { get; set; }
    public string? Relationship { get; set; }
    public string? Phone { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
