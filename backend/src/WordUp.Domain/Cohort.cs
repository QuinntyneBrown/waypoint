namespace WordUp.Domain;

/// <summary>
/// A training cohort. Full management lives in the ADMIN subsystem (M2); this minimal shape exists so
/// invitations and application approvals can reference a cohort + assigned mentor.
/// </summary>
public class Cohort
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public Guid? TrackId { get; set; }
    public DateTime TermStart { get; set; }
    public DateTime TermEnd { get; set; }
    public string? Location { get; set; }
    public Guid? MentorId { get; set; } // lead mentor
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
